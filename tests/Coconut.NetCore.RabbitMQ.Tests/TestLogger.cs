using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Coconut.NetCore.RabbitMQ.Tests;

internal sealed class TestLogger : ILogger, ILoggerFactory
{
    public ConcurrentQueue<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entries.Enqueue((logLevel, formatter(state, exception), exception));

    public ILogger CreateLogger(string categoryName) => this;

    public void AddProvider(ILoggerProvider provider) { }

    public void Dispose() { }
}
