using System;

namespace Coconut.NetCore.RabbitMQ.Internal
{
    internal static class WaitDurationProvider
    {
        public static readonly Func<int, TimeSpan> WaitUpTo30Seconds = attempt => TimeSpan.FromSeconds(Math.Min(attempt, 30));
    }
}
