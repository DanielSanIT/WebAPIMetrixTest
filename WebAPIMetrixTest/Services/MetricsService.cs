using System.Diagnostics;
using System.Diagnostics.Metrics;
using WebAPIMetrixTest.Controllers;
using WebAPIMetrixTest.Services.MericsHelper;

namespace WebAPIMetrixTest.Services
{
    public class MetricsService
    {
        private readonly ILogger<TestApiController> _log;
        private readonly DataService _dataService;
        private readonly MeasurementCache? _measurementCache;

        private static readonly Meter _meter = new("DemoMeter", "1.0.0");

        public Counter<int> ApiCallCounter { get; }
        public Counter<int> ErrorCounter { get; }
        public ObservableGauge<int> RandomIntGauge { get; }
        public ObservableGauge<int> RandomDataIntGauge { get; }

        public ObservableGauge<byte> AppAvailability { get; }

        private int _currentGaugeValue = 0;
        private IEnumerable<Measurement<byte>> appAvailability()
        {
            return _measurementCache.GetAppAvailability();
        }

        public Histogram<int> RndNumberHistogram { get; }
        public Histogram<long> RndTimeHistogram { get; }

        public Counter<int> LogCounter { get; }

        public MetricsService(ILogger<TestApiController> logger, DataService dataService, MeasurementCache measurementCache)
        {
            _log = logger;
            _dataService = dataService;
            _measurementCache = measurementCache;

            ApiCallCounter = _meter.CreateCounter<int>("calls", description: "Counts the number of API calls");
            ErrorCounter = _meter.CreateCounter<int>("errorss", description: "Counts the number of errors");
            RandomIntGauge = _meter.CreateObservableGauge<int>("RandomIntGauge", () => new(_currentGaugeValue), "Parrots", "Return the random number");
            RandomDataIntGauge = _meter.CreateObservableGauge("RandomDataIntGauge", _dataService.GetRandomDataNumber, "DBParrots", "Return the random number from DB");

            AppAvailability = _meter.CreateObservableGauge("AppAvailability", () => appAvailability(), description: "Return the random number");

            RndNumberHistogram = _meter.CreateHistogram<int>("Random-numbers", description: "Random numbers");

            RndTimeHistogram = _meter.CreateHistogram<long>("Random-times", "Seconds", "Random times");

            LogCounter = _meter.CreateCounter<int>("LogCounter", description: "Counts the number of logs");
        }

        public void SetRandomIntGauge(int value)
        {
            _currentGaugeValue = value;
        }

        public static IDisposable BeginTimedOperation(Histogram<long> histogram, TimeMetricType tmt = TimeMetricType.Miliseconds)
            => new TimedOperation(histogram, tmt);

        public class TimedOperation(Histogram<long> histogram, TimeMetricType tmt = TimeMetricType.Miliseconds) : IDisposable
        {
            readonly Stopwatch _sw = Stopwatch.StartNew();
            readonly Histogram<long> histogram = histogram;
            private readonly TimeMetricType tmt = tmt;

            public virtual void Dispose()
            {
                _sw.Stop();
                if (tmt == TimeMetricType.Seconds)
                    histogram.Record(_sw.ElapsedMilliseconds / 1000);
                else if (tmt == TimeMetricType.Minutes)
                    histogram.Record(_sw.ElapsedMilliseconds / 1000 / 60);
                else if (tmt == TimeMetricType.Hours)
                    histogram.Record(_sw.ElapsedMilliseconds / 1000 / 60 / 60);
                else
                    histogram.Record(_sw.ElapsedMilliseconds);
            }
        }

        public enum TimeMetricType
        {
            Miliseconds,
            Seconds,
            Minutes,
            Hours
        }
    }
}