using Microsoft.Extensions.Logging;
using MyStaging.Helpers;
using MyStaging.xUnitTest.DAL;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MyStaging.xUnitTest
{
    public class DeleteBuilderTest
    {
        public DeleteBuilderTest()
        {
            LoggerFactory factory = new LoggerFactory();
            var log = factory.CreateLogger<PgSqlHelper>();
            _startup.Init(log, ConstantUtil.CONNECTIONSTRING);
        }

        [Fact]
        public void Deleted()
        {
            string userid = "5b1b54bfd86b1b3bb0000009";
            var rows = User.DeleteBuilder.Where(f => f.Id == userid).SaveChange();
            Assert.Equal(1, rows);
        }
    }
}
