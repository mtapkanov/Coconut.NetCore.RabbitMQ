# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Opinionated wrapper over `RabbitMQ.Client` 7 for Microsoft.Extensions.Hosting. Three NuGet packages released together from one tag (they depend on each other with exact versions):

- `Coconut.NetCore.RabbitMQ.Core` - consumption event records (`MessageAcknowledgedEvent`, `MessageRejectedEvent`) and the `IRabbitMqEventHandler` extension point. No RabbitMQ.Client dependency.
- `Coconut.NetCore.RabbitMQ` - the bus: connections, declaration, publishing, consuming. Depends on Core only.
- `Coconut.NetCore.RabbitMQ.Metrics` - prometheus-net counters as an `IRabbitMqEventHandler`. Depends on Core. Optional for consumers of the bus.

`tools/HowToUse` is a runnable sample (Web SDK): publishes a message every 100 ms, consumes it back, exposes `/metrics`.

## Toolchain

- `net10.0` only, SDK pinned by `global.json` (rollForward latestFeature). `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `LangVersion latest` come from root `Directory.Build.props`.
- `src/Directory.Build.props`: package metadata, `IsAotCompatible=true` (trim/AOT analyzers are on, and warnings are errors: generic type parameters that reach DI registration need `[DynamicallyAccessedMembers(PublicConstructors)]`), XML docs required for public API.
- Central Package Management: versions only in `Directory.Packages.props`, `PackageReference` without `Version`.
- Solution file is `Coconut.NetCore.RabbitMQ.slnx`.
- Tests: xunit.v3 on Microsoft.Testing.Platform (opted in via `global.json` `test.runner`), `FakeTimeProvider`. Internal contracts are faked by hand (`tests/*/Fakes`), public/third-party interfaces (`IChannel`, `IRabbitMqEventHandler`) via NSubstitute. Do not add `InternalsVisibleTo DynamicProxyGenAssembly2`: mocking internals through a proxy library is intentionally not allowed. Test projects are executables; shared settings in `tests/Directory.Build.props`.

## Commands

```bash
dotnet build
dotnet test
dotnet test --project tests/Coconut.NetCore.RabbitMQ.Tests
dotnet test --project tests/Coconut.NetCore.RabbitMQ.Tests --filter-method "*QueueConsumerTests*"   # MTP filter syntax, not VSTest --filter
dotnet pack -c Release -p:Version=2.0.0 -o artifacts
dotnet format

# sample, needs a broker (appsettings.Development.json expects amqp://guest:guest@localhost:5672/test)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project tools/HowToUse
```

## Architecture

### Registration -> startup

1. `services.AddRabbitMq(uri, options => ...)` runs the builder lambda immediately and calls `Build()`: invalid configuration (exchange without `AcceptMessages`, queue without consumer/deserializer, zero prefetch) throws right there. Builders register serializers/deserializers as singletons and consumers as scoped (default) into `IServiceCollection` as a side effect.
2. Each call registers one `ConnectionDefinition` singleton = one broker connection. Shared singletons (`PublisherRegistry`, `RabbitMqEventBus`, `IRabbitMqBus`, `TimeProvider.System`, the hosted service) are `TryAdd`ed once.
3. `RabbitMqHostedService.StartAsync` (plain `IHostedService`, deliberately blocking) creates a `RabbitMqConnectionUnit` per definition: connect (own retry loop, backoff `min(attempt, 30)` s, only on `BrokerUnreachableException`), one publisher channel per exchange (declare + register publishers), one consumer channel per queue. Then `PublisherRegistry.Freeze()`. Because start blocks until connected, hosted services registered after `AddRabbitMq` can publish immediately; `PublishAsync` before freeze throws `InvalidOperationException`.
4. After the initial connect, recovery (connection, channels, QoS, consumers, topology) is RabbitMQ.Client automatic recovery, not library code.

### Generic definitions instead of reflection

`Internal/Definitions.cs`: builders produce `PublisherDefinition<TMessage, TSerializer>` and `QueueDefinition<TMessage>` holding factory delegates. The runtime path (`RabbitMqPublisher<T>`, `QueueConsumer<T>`) is fully generic: no `MakeGenericType`, no casts from `object`, no boxing. `PublisherRegistry` keys by `typeof(TMessage)` and stores `IRabbitMqPublisher<TMessage>[]` as the value, so the cast in `Get<T>()` is exact. Keep it that way when adding features; it is also what keeps the library AOT-clean.

### Publishing

`IRabbitMqBus.PublishAsync<T>` -> registry lookup by exact type (no base types/interfaces) -> every exchange that accepted `T`. `RabbitMqPublisher<T>` calls `BasicPublishAsync(mandatory: true)`. With `RabbitMqExchangeSettings.PublisherConfirms = true` (default) the channel has confirm tracking: the call completes on broker ack and throws `PublishException` on nack or return (`IsReturn` = unroutable). With confirms off, returns are only logged via `BasicReturnAsync`.

Every message gets fresh `BasicProperties` from `PublishPropertiesFactory<T>`: `message-id` = UUIDv7 (hex, from `TimeProvider`), `timestamp`, `delivery-mode` from `RabbitMqExchangeSettings.PersistentMessages` (default persistent), `content-type` from `IMessageSerializer<T>.ContentType` (default interface member, null = omitted), then the optional `configureProperties` callback from `AcceptMessages`. Don't cache/share the properties instance: the client may add tracing headers to it.

No lock around publish on purpose: in RabbitMQ.Client 7 a command is serialized into one buffer before the write, and the confirm semaphore covers only sequence assignment + write, not the confirm wait. Concurrent publishes on one channel are safe and pipelined (verified by decompiling 7.2.2).

### Consuming

`QueueConsumer<T>`: channel with `ConsumerDispatchConcurrency` from settings, `BasicQos(prefetch, global: false)` (global QoS is unsupported by quorum queues), optional declare + bindings, `AsyncEventingBasicConsumer`, `autoAck: false`. Per delivery (`HandleAsync`, internal for tests):
- deserialize `Body.Span` (only valid inside the callback) -> `ConsumeContext<T>` -> new async DI scope -> consumer `ConsumeAsync` -> dispose scope.
- Success -> `BasicAck` -> `MessageAcknowledgedEvent`.
- Failure (`context.Fail()` / `Failed = true`, or any exception including deserialization) -> `BasicNack(requeue)` with `requeue = context.Requeue ?? settings.RequeueOnFailure`. `OperationCanceledException` during shutdown always requeues. `requeue: false` goes to the DLX from `x-dead-letter-exchange` queue argument, or is dropped. Then `MessageRejectedEvent`, then `FailDelay` via `TimeProvider` only if requeued.
- Ack/nack failures (closed channel) are logged, not thrown: the broker redelivers unacked messages of a closed channel.

Shutdown order in `StopAsync`: `BasicCancel` all consumers -> cancel the stopping token (in-flight consumers see it and requeue) -> close channels -> close connections.

### Events and metrics

`RabbitMqEventBus` invokes all `IRabbitMqEventHandler`s sequentially on the consumer path after ack/nack, swallowing and logging handler exceptions. Events are immutable snapshots (queue, exchange, routing key, delivery tag, redelivered, `Data` from `ConsumeContext.Data`); they intentionally do not carry `BasicDeliverEventArgs`. Metrics package: `AddRabbitMqMetrics()` -> `app_rmq_messages_count_total{action="{exchange}.{routingKey}", result="success|failure"}`. `AddMetricsAppName` sets a global `app` static label and must run before any metric is created. The library does not expose `/metrics`.

### Logging

All log statements go through `[LoggerMessage]` source-generated methods in `Internal/Log.cs` (event ids grouped: 1-9 connection, 10-19 publish, 20-29 consume, 30+ events). Categories: `Coconut.NetCore.RabbitMQ.Connection|Publisher|Consumer`.

## Testing notes

- No broker needed. `QueueConsumerTests` drives `QueueConsumer<T>.HandleAsync` directly with a mocked `IChannel`; `RabbitMqPublisherTests` mocks `BasicPublishAsync<BasicProperties>`. Internals are visible to the test assemblies and `DynamicProxyGenAssembly2`.
- `FailDelay` is tested with `FakeTimeProvider`: don't replace `Task.Delay(..., timeProvider, ...)` with a plain delay.
- Metrics live in the static prometheus-net default registry: use unique label values per test.

## Known gaps

- No delivery-count based poison handling: with `RequeueOnFailure = true` and no per-message override a poison message is redelivered forever (quorum queues' `x-delivery-limit` is the broker-side fix).
- In-flight consumers are not awaited on shutdown; they are signalled via the token and the channel is closed after that.

## CI / release

`.github/workflows/coconut_netcore_rabbitmq.yml`: on PR, push to main and `v*.*.*` tags. Restore, build Release with `-p:Version`, test, pack all packable projects into `artifacts/`, push `.nupkg` (+ `.snupkg`) to nuget.org with `NUGET_API_KEY` only for tags. Version = tag without the leading `v`; other builds get `0.0.0-ci.<run>`. Local builds default to `0.0.0-local` (`src/Directory.Build.props`).
