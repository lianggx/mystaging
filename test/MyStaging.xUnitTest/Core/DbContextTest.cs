using MyStaging.Core;
using MyStaging.Metadata;
using MyStaging.xUnitTest.Models;
using Xunit;

namespace MyStaging.xUnitTest.Core
{
    public class MySqlDbContext : DbContext
    {
        public MySqlDbContext(StagingOptions options) : base(options, ProviderType.MySql)
        {
        }

        public DbSet<UserModel> User { get; set; }
    }

    public class DbContextTest
    {
        [Fact]
        public void Init()
        {
            var options = new MyStaging.Metadata.StagingOptions("MySql", ConstantUtil.MYSQL_CONNECTION);
            var context = new MySqlDbContext(options);

            Assert.NotNull(context.User);
        }

        [Fact]
        public void Refresh()
        {
            var options = new MyStaging.Metadata.StagingOptions("MySql", ConstantUtil.MYSQL_CONNECTION);
            var context = new MySqlDbContext(options);
            context.Refresh(ConstantUtil.MYSQL_CONNECTION, null);

            Assert.NotNull(context.User);
        }

        [Fact]
        public void Transaction()
        {
            var options = new MyStaging.Metadata.StagingOptions("MySql", ConstantUtil.MYSQL_CONNECTION);
            var context = new MySqlDbContext(options);
            context.BeginTransaction();
            Assert.Single(context.Trans);
            context.CommitTransaction();
            Assert.Equal(0, context.Trans.Keys.Count);

            context.BeginTransaction();
            Assert.Single(context.Trans);
            context.RollBackTransaction();
            Assert.Equal(0, context.Trans.Keys.Count);
        }
    }
}
