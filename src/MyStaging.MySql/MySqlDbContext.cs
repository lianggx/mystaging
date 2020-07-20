using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Metadata;
using System;

namespace MyStaging.MySql
{
    public class MySqlDbContext : DbContext
    {
        public MySqlDbContext(StagingOptions options) : base(options, ProviderType.MySql)
        {
        }
    }
}
