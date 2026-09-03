using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Godrej.Precheck.Service.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;

        // One lock per cache key so a miss on an expensive factory (e.g. the drawing-number
        // query) doesn't let every concurrent request run the factory at once (cache stampede).
        // Unrelated keys don't block each other.
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getFunction, TimeSpan duration)
        {
            try
            {
                if (_cache.TryGetValue(key, out T cachedValue))
                {
                    return cachedValue;
                }

                var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
                await keyLock.WaitAsync();
                try
                {
                    // Re-check: another request may have populated the cache while we waited.
                    if (_cache.TryGetValue(key, out cachedValue))
                    {
                        return cachedValue;
                    }

                    cachedValue = await getFunction();

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(duration)
                        .SetPriority(CacheItemPriority.Normal)
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _logger.LogInformation($"Cache entry {key} was evicted due to {reason}");
                        });

                    _cache.Set(key, cachedValue, cacheOptions);
                    return cachedValue;
                }
                finally
                {
                    keyLock.Release();
                }
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
