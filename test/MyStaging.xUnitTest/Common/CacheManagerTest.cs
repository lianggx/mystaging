using MyStaging.Common;
using MyStaging.Metadata;
using MyStaging.xUnitTest.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Xunit;

namespace MyStaging.xUnitTest.Common
{
    public class CacheManagerTest
    {
        private readonly CacheManager cacheManager;
        public CacheManagerTest()
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
                Id = 1,
                Age = 18,
            };

            cacheManager.SetItemCache(user);
        }

        [Fact]
        public void GetItemCache()
        {
            var user = new UserModel
            {
                Id = 1,
                Age = 18,
            };

            IList<DbParameter> parameters = new List<DbParameter>();
            user = cacheManager.GetItemCache<UserModel, DbParameter>(parameters);
        }
    }
}
