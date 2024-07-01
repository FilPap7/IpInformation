using System;
using System.Runtime.Caching;

namespace Common.Cache
{
    public class CacheService : ICacheService
    {
        private ObjectCache _cache = MemoryCache.Default;

        public T GetData<T>(string key)
        {
            try
            { 
                return (T)_cache.Get(key);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception found: {ex}");
            }
        }

        public void SetData<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                if (!string.IsNullOrEmpty(key) && value != null)
                {
                    var policy = new CacheItemPolicy
                    {
                        SlidingExpiration = expiration
                    };
                    _cache.Set(key, value, policy);
                }
                else
                {
                    throw new Exception("Key or Value is null or empty");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception found: {ex}");
            }
        }

        public void RemoveData(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _cache.Remove(key);
                }
                else
                {
                    throw new Exception("Key or Value is null or empty");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception found: {ex}");
            }
        }
    }
}
