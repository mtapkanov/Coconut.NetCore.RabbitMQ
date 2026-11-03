using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coconut.NetCore.RabbitMQ.Internal;

/// <summary>
///     Connects every configured broker before the host finishes starting, so <see cref="IRabbitMqBus" />
///     is usable as soon as the application serves requests. With an unreachable broker the host start blocks
///     (retrying) until it connects or the start is cancelled.
/// </summary>
internal sealed class RabbitMqHostedService(
    IEnumerable<ConnectionDefinition> definitions,
    IServiceProvider services,
    PublisherRegistry registry,
    RabbitMqEventBus eventBus,
    TimeProvider timeProvider,
    ILoggerFactory loggerFactory) : IHostedService, IAsyncDisposable
{
    private readonly CancellationTokenSource _stopping = new();
    private readonly List<RabbitMqConnectionUnit> _units = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var definition in definitions)
        {
            var unit = new RabbitMqConnectionUnit(definition, services, registry, eventBus, timeProvider, loggerFactory);
            _units.Add(unit);
            await unit.StartAsync(cancellationToken, _stopping.Token).ConfigureAwait(false);
        }

        registry.Freeze();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // 1. Stop new deliveries. 2. Interrupt in-flight consumers (their messages are requeued). 3. Close channels and connections.
        foreach (var unit in _units)
            await unit.CancelConsumersAsync(cancellationToken).ConfigureAwait(false);

        await _stopping.CancelAsync().ConfigureAwait(false);

        foreach (var unit in _units)
            await unit.DisposeAsync().ConfigureAwait(false);

        _units.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var unit in _units)
            await unit.DisposeAsync().ConfigureAwait(false);

        _stopping.Dispose();
    }
}
