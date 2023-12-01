using System.Diagnostics.Metrics;

namespace WebAPIMetrixTest.Services.MericsHelper
{
    public class MeasurementCache
    {
        private IEnumerable<Measurement<byte>> _cachedMeasurements;
        private DateTime? _lastUpdated;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);  // Кеш длится 5 минут, можете изменить это значение

        // Объект для синхронизации потоков
        private readonly object _cacheLock = new();

        public MeasurementCache()
        {
            // Загрузите изначальные данные при инициализации, если это нужно.
            _cachedMeasurements = appAvailability();
        }

        public IEnumerable<Measurement<byte>> GetAppAvailability()
        {
            lock (_cacheLock)
            {
                // Если кеш существует и еще актуален, возвращаем его.
                if (_cachedMeasurements != null && _lastUpdated.HasValue && DateTime.Now - _lastUpdated.Value <= CacheDuration)
                {
                    return _cachedMeasurements;
                }

                // Если кеш не существует или устарел, запускаем обновление кеша асинхронно.
                Task.Run(() => UpdateCacheAsync());

                if (_cachedMeasurements != null)
                {
                    return _cachedMeasurements;
                }

                // Если кеша не было совсем, возвращаем изначальные данные.
                return appAvailability();
            }
        }

        private async Task UpdateCacheAsync()
        {
            lock (_cacheLock)
            {
                _cachedMeasurements = appAvailability();
                _lastUpdated = DateTime.Now;
            }

            await Task.CompletedTask;  // Это здесь только для демонстрации. Если у вас есть другой асинхронный код, выполните его здесь.
        }

        private static IEnumerable<Measurement<byte>> appAvailability()
        {
            List<Measurement<byte>> apps = new()
            {
                new Measurement<byte>(1, new KeyValuePair<string, object?>("app", "RabbitMQ")),
                new Measurement<byte>(0, new KeyValuePair<string, object?>("app", "Integra")),
                new Measurement<byte>(1, new KeyValuePair<string, object?>("app", "S3")),
                new Measurement<byte>(1, new KeyValuePair<string, object?>("app", "MSSQL"))
            };
            return apps;
        }
    }
}
