using Microsoft.Extensions.Logging;
using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.xUnitTest.DAL;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MyStaging.xUnitTest
{
    public class UpdateBuilderTest
    {
        public UpdateBuilderTest()
        {
            LoggerFactory factory = new LoggerFactory();
            var log = factory.CreateLogger<PgSqlHelper>();
            var options = new StagingOptions()
            {
                ConnectionMaster = ConstantUtil.CONNECTIONSTRING,
                Logger = log
            };
            _startup.Init(options);
        }

        [Fact]
        public void Update()
        {
            string userid = "5b1b54bfd86b1b3bb0000009";
            var user = User.Context.Where(f => f.Id == userid).ToOne();
            var builder = user.UpdateBuilder;
            builder.SetMoney(2000);
            var sql = builder.ToString();
            builder.SaveChange();

            var rows = User.UpdateBuilder.Where(f => f.Id == userid).SetMoney(2000).SetSex(false).SaveChange();

            Assert.Equal(1, rows);
        }
    }
}
