using System;
using System.Text;
using Npgsql;
using System.Linq;

using System.Collections.Generic;
using System.Threading;
using System.Linq.Expressions;
using MyStaging.Common;
using MyStaging.Helpers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
//using gmall.DAL;
//using gmall.Model;
//using gmall;
using MyStaging;
//using gmall.Model;
//using gmall.DAL;

namespace MyStaging.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                var item = args[i].ToLower();
                if (item == "-h")
                    sb.Append($"host={args[i + 1]};");
                else if (item == "-p")
                    sb.Append($"port={args[i + 1]};");
                else if (item == "-u")
                    sb.Append($"username={args[i + 1]};");
                else if (item == "-a")
                    sb.Append($"password={args[i + 1]};");
                else if (item == "-d")
                    sb.Append($"database={args[i + 1]};");
                i++;
            }
            string connectionString = "Host=172.16.1.220;Port=5432;Username=postgres;Password=123456;Database=superapp;Maximum Pool Size=50";
            NLog.LogFactory logf = new NLog.LogFactory();
            NLog.Logger logger = logf.CreateNullLogger();

            PgSqlHelper.InitConnection(logger, connectionString);
            SchemaFactory.Start("gmall");

            //gmall._startup.Init(logger, connectionString);
            //TestExpression();

            Console.WriteLine("Hello World!");


            Console.ReadKey();
        }

        class u_model
        {
            public Guid id { get; set; }
            public string title { get; set; }
        }

        static void TestExpression()
        {
            //Guid mid = Guid.Parse("70981417-88f2-4c07-b326-674993fa4b14");
            //UserDal user = new UserDal();
            //var result = user.Where(f => f.Id == mid).ToOne();
            //int affrows = UserDal.Update(mid)
            //                     .SetNickName("name")
            //                     .SaveChange();



            //u_model userModel = Public_guser.Context
            //    .Union<Public_appModel>("b", UnionType.INNER_JOIN, (a, b) => a.Id == b.Guser_id)
            //    .Union<Public_appModel, Public_appmoduleModel>("c", UnionType.LEFT_JOIN, (a, b) => a.Id == b.App_id)
            //    .Where<Public_appModel>(f => f.Guser_id == Guid.Parse("d3a35954-e5bb-406b-8300-dd83ae859b86"))
            //    .ToOne<u_model>("a.id,b.title");

            //Public_guserModel model = Public_guser.Context.Where(f => f.Phone == "1180000000").ToOne();
            //int count = Public_guser.Delete(model.Id);
            //int affrows = Public_guser.Update(model.Id).SetState(Et_proxy_state.冻结).SaveChange();
            //Public_guserModel user = new Public_guserModel();
            //user.Id = Guid.NewGuid();
            //user.Phone = "1180000000";
            //user.Create_time = DateTime.Now;
            //user.Reg_time = DateTime.Now;
            //user.Login_time = DateTime.Now;
            //user.State = Et_proxy_state.正常;
            //var um = Public_guser.Insert(user);
            // List<Mall_productModel> model = Mall_product.Context.Page(1, 10).OrderBy(f => f.Create_time).ToList();

            Console.ReadKey();
        }

        static void Test()
        {
            //for (int i = 0; i < 2; i++)
            //{

            //    Thread thre = new Thread(
            //        new ThreadStart(
            //            () =>
            //            {
            //                for (int j = 0; j < 1; j++)
            //                {
            //                    List<UserModel> list = new List<UserModel>();
            //                    User user = new User();
            //                    Guid userid = Guid.Parse("70981417-88f2-4c07-b326-674993fa4b14");
            //                    UserModel model = user.WhereId(userid, ConditionType.NotEqual)
            //                        .Union<User>(UnionType.INNER_JOIN, "b", "b.id=a.id")
            //                        .GroupBy("a.id,a.nickname,b.id,b.nickname")
            //                        .Having("count(a.id)=1")
            //                        .ToOne();
            //                    User<UserModel2> user2 = new User<UserModel2>();



            //                    User.Update(model.Id).SetId(null).SetName("").SaveChange();
            //                    UserModel2 model2 = user2.WhereId(userid, ConditionType.NotEqual)
            //                                           .Union<User>(UnionType.INNER_JOIN, "b", "b.id=a.id")
            //                                           .GroupBy("a.id,a.nickname,b.id,b.nickname")
            //                                           .Having("count(a.id)=1")
            //                                           .ToOne();
            //                    Console.WriteLine(model2.NickName);
            //                }
            //            }
            //            )
            //    );
            //    thre.Start();

            //}
        }
    }
}





