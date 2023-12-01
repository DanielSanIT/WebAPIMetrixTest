using Serilog.Core;
using Serilog.Events;

namespace WebAPIMetrixTest.Services.MericsHelper
{
    public class MetricsSink(MetricsService meter) : ILogEventSink
    {
        private readonly MetricsService _meter = meter;

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level == LogEventLevel.Verbose)
                _meter.LogCounter.Add(1, new KeyValuePair<string, object?>("name", "Verbose"));
            else if (logEvent.Level == LogEventLevel.Debug)
                _meter.LogCounter.Add(1, new KeyValuePair<string, object?>("name", "Debug"));
            else if (logEvent.Level == LogEventLevel.Information)
                _meter.LogCounter.Add(1, new KeyValuePair<string, object?>("name", "Information"));
            else if (logEvent.Level == LogEventLevel.Warning)
                _meter.LogCounter.Add(1, new KeyValuePair<string, object?>("name", "Warning"));
            else if (logEvent.Level == LogEventLevel.Error)
                _meter.LogCounter.Add(1, new KeyValuePair<string, object?>("name", "Error"));
            else
                _meter.LogCounter.Add(1, new KeyValuePair<string, object?>("name", "Fatal"));
        }
    }
}