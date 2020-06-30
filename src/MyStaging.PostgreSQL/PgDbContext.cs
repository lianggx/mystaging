using MyStaging.Common;
using MyStaging.Core;
using System;

namespace MyStaging.PostgreSQL
{
    public class PgDbContext : DbContext
    {
        public PgDbContext(StagingOptions options) : base(options, ProviderType.PostgreSQL)
        {
        }
    }
}
