using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Infrastructure.Caching
{
    public class CachedRepositoryService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

        public CachedRepositoryService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // دالة  لجلب البيانات مع الكاش
        public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> queryFactory, TimeSpan? expiration = null)
        {
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration;
                return await queryFactory();
            }) ?? default!;
        }


        // دالة  لمسح الكاش عند التعديل (Invalidation)
        public void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
    }
}
