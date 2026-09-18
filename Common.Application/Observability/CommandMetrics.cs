using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace Common.Application.Observability
{
    public static class CommandMetrics
    {
        private static readonly Meter Meter = new("VOEConsulting.Flame.Common.Commands");

        public static readonly Counter<long> Attempts = Meter.CreateCounter<long>("command.attempts");
        public static readonly Counter<long> Success = Meter.CreateCounter<long>("command.success");
        public static readonly Counter<long> Failures = Meter.CreateCounter<long>("command.failures");
        public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("command.duration");
    }
}
