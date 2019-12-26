using Microsoft.Extensions.Logging;
using MyStaging.Common;
using MyStaging.Helpers;
using MyStaging.xUnitTest.DAL;
using MyStaging.xUnitTest.Model;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
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
            for (int i = 0; i < 1; i++)
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
                    Sex = true,
                    Wealth = 100
                }.Insert();

                model.UpdateBuilder.SetWealth(200).SaveChange();
                model.UpdateBuilder.SetIncrement("wealth", 1).SaveChange();
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


        [Fact]
        public void Transaction()
        {
            UserModel user = null;
            UserModel result = null;
            DbTransaction tran = null;
            DbConnection conn = null;
            ConcurrentDictionary<int, DbTransaction> trans = null;

            try
            {
                tran = PgSqlHelper.InstanceMaster.BeginTransaction();
                conn = tran.Connection;
                trans = PgSqlHelper.InstanceMaster.Trans;
                user = new UserModel()
                {
                    Age = 18,
                    Createtime = DateTime.Now,
                    Id = ObjectId.NewId().ToString(),
                    Loginname = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Money = 0,
                    Nickname = "北极熊",
                    Password = "123456",
                    Sex = true
                };
                result = User.Insert(user);
                throw new ArgumentNullException();
                PgSqlHelper.InstanceMaster.CommitTransaction();
            }
            catch (Exception ex)
            {
                PgSqlHelper.InstanceMaster.RollBackTransaction();
            }
            finally
            {

            }
            Assert.Equal(user.Id, result.Id);
        }
    }
}
