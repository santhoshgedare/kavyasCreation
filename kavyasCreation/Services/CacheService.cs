using Microsoft.Extensions.Caching.Memory;

namespace kavyasCreation.Services
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null);
        void Remove(string key);
        bool Exists(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T? Get<T>(string key)
        {
            try
            {
                return _cache.TryGetValue(key, out T? value) ? value : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key {Key}", key);
                return default;
            }
        }

        public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    cacheOptions.AbsoluteExpirationRelativeToNow = absoluteExpiration;
                }
                else
                {
                    cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                _cache.Set(key, value, cacheOptions);
                _logger.LogDebug("Cache set for key {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key {Key}", key);
            }
        }

        public void Remove(string key)
        {
            try
            {
                _cache.Remove(key);
                _logger.LogDebug("Cache removed for key {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key {Key}", key);
            }
        }

        public bool Exists(string key)
        {
            return _cache.TryGetValue(key, out _);
        }
    }
}
