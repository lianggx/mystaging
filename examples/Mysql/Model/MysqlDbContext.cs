using Mysql.Model;
using MyStaging.Core;
using MyStaging.Metadata;

namespace Mysql
{
    public partial class MysqlDbContext : DbContext
	{
		public MysqlDbContext(StagingOptions options) : base(options, ProviderType.MySql)
		{
		}

		public DbSet<Article> Article { get; set; }
		public DbSet<Customer> Customer { get; set; }
		public DbSet<M_Accesslog> M_Accesslog { get; set; }
		public DbSet<M_Mapping> M_Mapping { get; set; }
		public DbSet<M_Resource> M_Resource { get; set; }
		public DbSet<M_Role> M_Role { get; set; }
		public DbSet<M_Resource> M_Roleresource { get; set; }
		public DbSet<M_User> M_User { get; set; }
	}
}
