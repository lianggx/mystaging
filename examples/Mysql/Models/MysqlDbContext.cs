using Mysql.Model;
using System;
using MyStaging.Core;
using MyStaging.Common;
using Newtonsoft.Json.Linq;

namespace Mysql
{
	public class MysqlDbContext : DbContext
	{
		public MysqlDbContext(StagingOptions options) : base(options, ProviderType.MySql)
		{
		}

		public DbSet<Article> Article { get; set; }
		public DbSet<M_type> M_type { get; set; }
	}
}
