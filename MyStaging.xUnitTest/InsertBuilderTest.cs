using Microsoft.Extensions.Logging;
using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.xUnitTest.DAL;
using MyStaging.xUnitTest.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MyStaging.xUnitTest
{
    public class InsertBuilderTest
    {
        protected static ITestOutputHelper output = null;

        public InsertBuilderTest(ITestOutputHelper outPut)
        {
            output = outPut;
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
        public void Insert()
        {
            for (int i = 0; i < 10; i++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var model = new UserModel()
                {
                    Age = 18,
                    Createtime = DateTime.Now,
                    Id = ObjectId.NewId().ToString(),
                    Loginname = Guid.NewGuid().ToString("N"),
                    Money = 100,
                    Nickname = Guid.NewGuid().ToString("N"),
                    Password = "123456",
                    Sex = true
                }.Insert();
                sw.Stop();

                output.WriteLine("执行时间：{0}", sw.ElapsedMilliseconds);
                Assert.NotNull(model);
            }
        }

        [Fact]
        public void InsertRange()
        {
            int total = 500;
            List<UserModel> list = new List<UserModel>();
            for (int i = 0; i < total; i++)
            {
                var model = new UserModel()
                {
                    Age = 18,
                    Createtime = DateTime.Now,
                    Id = ObjectId.NewId().ToString(),
                    Loginname = Guid.NewGuid().ToString("N"),
                    Money = 100,
                    Nickname = Guid.NewGuid().ToString("N"),
                    Password = "123456",
                    Sex = true
                };

                list.Add(model);
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var rows = User.InsertRange(list);
            sw.Stop();

            output.WriteLine("执行时间：{0}", sw.ElapsedMilliseconds);
            Assert.Equal(total, rows);
        }

    }
}
