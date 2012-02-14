namespace KhanProxy.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Caching;

    public static class CacheHelper
    {
        private static Cache cache = System.Web.HttpRuntime.Cache;

        public static T Get<T, K>(string cacheKeyPrefix, IEnumerable<K> keyList, Func<T> action, DateTime expiration)
        {
            StringBuilder keyPostFix = keyList.Aggregate(new StringBuilder(), (current, next) => current.Append(":").Append(next));
            return Get(cacheKeyPrefix + keyPostFix, action, expiration);
        }

        public static T Get<T>(string cacheKey, Func<T> action, DateTime expiration)
        {
            return Get<T>(cacheKey, action, expiration, Cache.NoSlidingExpiration);
        }

        public static T Get<T>(string cacheKey, Func<T> action, DateTime expiration, TimeSpan sliding)
        {
            string boolCacheKey = "b" + cacheKey;

            if (cache == null) return action();

            object cached = cache[cacheKey];
            var cachebool = cache[boolCacheKey] as CachePlaceholder;

            // first check the cachebool
            if (cachebool != null && !cachebool.Available)
            {
                // the placeholder was there, but did not return anything
                // short circuit until this placeholder's cache entry expires
                return default(T);
            }

            if (cached == null)
            {
                cached = action();
                if (cached != null)
                {
                    cache.Add("b" + cacheKey, new CachePlaceholder { Available = true }, null, expiration, sliding, CacheItemPriority.Normal, null);

                    cache.Add(
                        cacheKey,
                        cached,
                        null,
                        expiration,
                        sliding,
                        CacheItemPriority.Normal,
                        null);
                }
                else
                {
                    // it was not found, put a false bool in the cache 
                    cache.Add("b" + cacheKey, new CachePlaceholder { Available = false }, null, expiration, sliding, CacheItemPriority.Normal, null);
                }
            }
            return (T)cached;
        }

        /// <summary>Empty</summary>
        public class CachePlaceholder
        {
            public bool Available;
        }
    }
}
