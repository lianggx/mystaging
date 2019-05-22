using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Common
{
    public class StagingOptions
    {
        public ILogger Logger { get; set; }
        public string ConnectionMaster { get; set; }
        public string[] ConnectionSlaves { get; set; }
        public int SlavesMaxPool { get; set; } = -1;
        public CacheOptions CacheOptions { get; set; }
    }

    public class CacheOptions
    {
        /// <summary>
        ///  缓存前缀，默认值：mystaging_
        /// </summary>
        public string Prefix { get; set; } = "mystaging_";
        public IDistributedCache Cache { get; set; }
        /// <summary>
        ///  默认过期时间 60s,AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        /// </summary>
        public DistributedCacheEntryOptions Options { get; set; } = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };
    }
}
