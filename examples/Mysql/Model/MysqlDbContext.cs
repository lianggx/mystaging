using Mysql.Model;
using System;
using MyStaging.Core;
using MyStaging.Common;
using MyStaging.Metadata;
using System.Text.Json;

namespace Mysql
{
	public partial class MysqlDbContext : DbContext
	{
		public MysqlDbContext(StagingOptions options) : base(options, ProviderType.MySql)
		{
		}

		public DbSet<Article> Article { get; set; }
		public DbSet<Customer> Customer { get; set; }
	}
}
