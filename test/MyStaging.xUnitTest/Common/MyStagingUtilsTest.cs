using MyStaging.Common;
using MyStaging.Metadata;
using MyStaging.xUnitTest.Models;
using Xunit;

namespace MyStaging.xUnitTest.Common
{
    public class MyStagingUtilsTest
    {
        [Fact]
        public void GetDbFields()
        {
            var pis = MyStagingUtils.GetDbFields(typeof(UserModel));
            Assert.Equal(10, pis.Count);
        }

        [Fact]
        public void GetMapping()
        {
            var mysqlName = "`mystaging`.`user`";
            var mysql = MyStagingUtils.GetMapping(typeof(UserModel), Metadata.ProviderType.MySql);
            Assert.Equal(mysqlName, mysql);

            var pgsqlName = "\"mystaging\".\"user\"";
            var pgsql = MyStagingUtils.GetMapping(typeof(UserModel), Metadata.ProviderType.PostgreSQL);
            Assert.Equal(pgsqlName, pgsql);
        }

        [Fact]
        public void GetTableName()
        {
            var table = new TableInfo
            {
                Schema = "mystaging",
                Name = "user"
            };
            var mysqlName = "`mystaging`.`user`";
            var mysql = MyStagingUtils.GetTableName(table, Metadata.ProviderType.MySql);
            Assert.Equal(mysqlName, mysql);

            var pgsqlName = "\"mystaging\".\"user\"";
            var pgsql = MyStagingUtils.GetTableName(table, Metadata.ProviderType.PostgreSQL);
            Assert.Equal(pgsqlName, pgsql);
        }

        [Fact]
        public void CopyProperty()
        {
            var source = new UserModel
            {
                Id = 1,
                Age = 18,
            };
            UserModel target = new UserModel();
            MyStagingUtils.CopyProperty(target, source);
            Assert.Equal(source.Id, target.Id);
        }

        [Fact]
        public void GetValueTuple()
        {

        }

        [Fact]
        public void GetMemberName()
        {
            var source = new UserModel
            {
                Id = 1,
                Age = 18,
            };
            UserModel target = new UserModel();
            var memberName = MyStagingUtils.GetMemberName<UserModel, int>(f => f.Id);
            Assert.Equal("Id", memberName);
        }

        [Fact]
        public void ToUpperPascal()
        {
            var field = "Name";
            var cast = MyStagingUtils.ToUpperPascal("name");
            Assert.Equal(field, cast);
        }

        [Fact]
        public void ToLowerPascal()
        {
            var field = "name";
            var cast = MyStagingUtils.ToLowerPascal("Name");
            Assert.Equal(field, cast);
        }
    }
}
