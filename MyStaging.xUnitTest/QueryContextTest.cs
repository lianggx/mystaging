using System;
using Xunit;
using MyStaging.Helpers;
using Microsoft.Extensions.Logging;
using MyStaging.xUnitTest;
using MyStaging.xUnitTest.Model;
using MyStaging.xUnitTest.DAL;
using MyStaging.Common;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;

namespace MyStaging.xUnitTest
{
    public class QueryContextTest
    {
        public QueryContextTest()
        {
            LoggerFactory factory = new LoggerFactory();
            var log = factory.CreateLogger<PgSqlHelper>();
            _startup.Init(log, ConstantUtil.CONNECTIONSTRING);
        }

        private string Sha256Hash(string text)
        {
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        static int num = 0;
        [Fact(Skip = "需要手动运行该测试")]
        public void InsertTest()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread thr = new Thread(new ThreadStart(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        UserModel user = new UserModel()
                        {
                            Age = 18,
                            Createtime = DateTime.Now,
                            Id = ObjectId.NewId().ToString(),
                            Loginname = Guid.NewGuid().ToString("N").Substring(0, 8),
                            Money = 0,
                            Nickname = "北极熊",
                            Password = Sha256Hash("123456"),
                            Sex = true
                        };
                        var result = User.Insert(user);
                        Assert.Equal(user.Id, result.Id);
                    }
                    num++;
                }))
                {
                    IsBackground = true
                };
                thr.Start();
            }
            while (num < 10)
            {
                Thread.Sleep(1000);
            }
        }

        [Fact]
        public void Insert()
        {
            UserModel user = new UserModel()
            {
                Age = 18,
                Createtime = DateTime.Now,
                Id = ObjectId.NewId().ToString(),
                Loginname = Guid.NewGuid().ToString("N").Substring(0, 8),
                Money = 0,
                Nickname = "北极熊",
                Password = Sha256Hash("123456"),
                Sex = true
            };
            var result = User.Insert(user);
            Assert.Equal(user.Id, result.Id);
        }

        [Fact]
        public void ToList()
        {
            var list = User.Context.OrderByDescing(f => f.Createtime).Page(1, 10).ToList();

            Assert.Equal(10, list.Count);
        }

        [Fact]
        public void ToListValueType()
        {
            var list = User.Context.OrderByDescing(f => f.Createtime).Page(1, 10).ToList<string>("id");

            Assert.Equal(10, list.Count);
        }

        [Fact]
        public void ToListValueTulpe()
        {
            var list = User.Context.OrderByDescing(f => f.Createtime).Page(1, 10).ToList<(string id, string loginname, string nickname)>("id", "loginname", "nickname");

            Assert.Equal(10, list.Count);
        }

        [Fact]
        public void ToOne()
        {
            string hash = Sha256Hash("123456");
            var user = User.Context.OrderBy(f => f.Createtime).ToOne();

            Assert.Equal(hash, user.Password);
        }

        [Fact]
        public void ToScalar()
        {
            string hash = Sha256Hash("123456");
            var password = User.Context.Where(f => f.Password == hash).OrderBy(f => f.Createtime).ToScalar<string>("password");

            Assert.Equal(hash, password);
        }

        [Fact]
        public void Sum()
        {
            int total = 360;
            // 先把数据库任意两条记录修改为 180 
            var age = User.Context.Where(f => f.Age == 180).Sum<long>(f => f.Age);

            Assert.Equal(total, age);
        }

        [Fact]
        public void Avg()
        {
            decimal avg = 180;
            // 先把数据库任意两条记录的 age 字段修改为 180 
            var age = User.Context.Where(f => f.Age == 180).Avg<decimal>(f => f.Age);

            Assert.Equal(avg, age);
        }

        [Fact]
        public void Count()
        {
            int count = 2;
            // 先把数据库任意两条记录的 age 字段修改为 180 
            var age = User.Context.Where(f => f.Age == 180).Count();

            Assert.Equal(count, age);
        }

        [Fact]
        public void Max()
        {
            int max = 180;
            var age = User.Context.Max<int>(f => f.Age);

            /// 上面插入数据库的 age 字段是 180
            Assert.Equal(max, age);
        }

        [Fact]
        public void Min()
        {
            int min = 18;
            var age = User.Context.Min<int>(f => f.Age);

            /// 上面插入数据库的 age 字段是 18
            Assert.Equal(min, age);
        }
    }
}
