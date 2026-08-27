using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Service.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, TimeSpan duration)
        {
            try
            {
                if (!_cache.TryGetValue(key, out T cachedValue))
                {
                    cachedValue = await getFunction();

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(duration)
                        .SetPriority(CacheItemPriority.Normal)
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _logger.LogInformation($"Cache entry {key} was evicted due to {reason}");
                        });

                    _cache.Set(key, cachedValue, cacheOptions);
                }

                return cachedValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in cache operation for key: {key}");
                throw;
            }
        }



        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
