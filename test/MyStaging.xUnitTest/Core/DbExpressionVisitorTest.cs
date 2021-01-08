using MyStaging.Core;
using MyStaging.xUnitTest.Models;
using System;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace MyStaging.xUnitTest.Core
{
    public class DbExpressionVisitorTest
    {
        private readonly ITestOutputHelper testOutput;
        public DbExpressionVisitorTest(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        [Fact]
        public void Visit()
        {
            var options = new MyStaging.Metadata.StagingOptions("MySql", ConstantUtil.MYSQL_CONNECTION);
            var context = new MySqlDbContext(options);

            Expression<Func<UserModel, bool>> predicate = (f) => f.Id == 1;
            DbExpressionVisitor exp = new DbExpressionVisitor
            {
                TypeMaster = typeof(UserModel),
                AliasMaster = "a",
                AliasUnion = null
            };

            exp.Visit(predicate);
            var sql = exp.SqlText.Builder.ToString();
            testOutput.WriteLine(sql);

            Assert.StartsWith("(a.Id = @", sql);
            var count = exp.SqlText.Parameters.Count;
            Assert.Equal(1, count);

            predicate = (f) => f.Id == 1 && f.IP == "127.0.0.1";
            exp = new DbExpressionVisitor
            {
                TypeMaster = typeof(UserModel),
                AliasMaster = "a",
                AliasUnion = null
            };

            exp.Visit(predicate);
            sql = exp.SqlText.Builder.ToString();
            testOutput.WriteLine(sql);

            Assert.StartsWith("((a.Id = @", sql);
            Assert.EndsWith("))", sql);
            count = exp.SqlText.Parameters.Count;
            Assert.Equal(2, count);
        }
    }
}
