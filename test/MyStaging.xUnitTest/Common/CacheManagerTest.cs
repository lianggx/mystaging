using MyStaging.Common;
using MyStaging.xUnitTest.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MyStaging.xUnitTest.Common
{
    public class CacheManagerTest
    {
        private readonly CacheManager cacheManager;
        public CacheManagerTest(CacheManager cacheManager)
        {
            CacheOptions options = new CacheOptions()
            {
                Cache = new Microsoft.Extensions.Caching.Redis.CSRedisCache(new CSRedis.CSRedisClient(ConstantUtil.REDIS_CONNECTION))
            };
            cacheManager = new CacheManager(options);
        }

        [Fact]
        public void SetItemCache()
        {
            var user = new UserModel
            {
                id = "5dd8e7f9b5ee486b0c000001",
                age = 18,
            };

            cacheManager.SetItemCache(user);
        }

        [Fact]
        public void GetItemCache()
        {
            var user = new UserModel
            {
                id = "5dd8e7f9b5ee486b0c000001",
                age = 18,
            };

         //   cacheManager.GetItemCache(user);
        }
    }
}
