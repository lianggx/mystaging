using MyStaging.Common;
using MyStaging.Core;
using System;

namespace MyStaging.PostgreSQL
{
    public class PgDbContext : DbContext
    {
        public PgDbContext(StagingOptions options) : base(options)
        {

        }

        public override void Dispose()
        {
        }
    }
}
