using Mysql.Model;
using MyStaging.Common;
using MyStaging.Interface;
using MyStaging.Metadata;
using MyStaging.MySql.Generals;
using Newtonsoft.Json.Linq;
using System;

namespace Mysql
{
    public class Program
    {
        static void Main(string[] args)
        {
            //IGeneralFactory factory = new GeneralFactory();
            //factory.CodeFirst(new MyStaging.Metadata.ProjectConfig()
            //{
            //    ConnectionString = "server=127.0.0.1;user id=root;password=root;database=mystaging",
            //    Mode = MyStaging.Metadata.GeneralMode.Db,
            //    OutputDir = @"D:\MyGitHub\mystaging\examples\Mysql\Models",
            //    ProjectName = "Mysql",
            //    Provider = "MySql"
            //});

            var options = new MyStaging.Metadata.StagingOptions("MySql", "server=127.0.0.1;user id=root;password=root;");
            var context = new MysqlDbContext(options);

            var customer = new Customer { Name = "好久不见" };

            try
            {
                // 测试事务
                context.BeginTransaction();
                context.Customer.Insert.Add(customer);
                context.CommitTransaction();

                var nc = context.Customer.Select.Where(f => f.Id == customer.Id).ToOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //// 单个查询
            //var article = context.Customer.Select.Where(f => f.Id == 2 && f.Name == "Ron").ToOne();
            //// 列表查询，排序、分页、分组
            //var articles = context.Customer.Select.OrderBy(f => f.Id).Page(1, 10).GroupBy("Name").ToList();
            //// 表连接查询
            //var article = context.Article.Select.InnerJoin<Customer>("b", (a, b) => a.userid == b.Id).Where<Customer>(f => f.Id == 2).ToOne();
            //// 首字段查询，ToScalar 参数可以传递 Sql 参数，比如 SUM(x)
            //var id = context.Customer.Select.Where(f => f.Id == 2 && f.Name == "Ron").ToScalar<int>("Id");
            var a3 = context.Article.Update.SetValue(f => f.content, "未来已来，从这里开始").Where(f => f.id == 1).SaveChange();
            var article = new Article()
            {
                content = "你是谁？你从哪里来？要到哪里去？",
                createtime = DateTime.Now,
                userid = customer.Id,
                IP = "127.0.0.1",
                State = true,
                title = "振聋发聩的人生三问"
            };

            var list = new System.Collections.Generic.List<Article>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(article);
            }
            var a2 = context.Article.Insert.Add(article);
            var affrows = context.Article.Insert.AddRange(list).SaveChange();
            Console.WriteLine(affrows);
            // context.Article.Delete.Where(f => f.id == a2.id).SaveChange();

            //    Console.WriteLine(a2.id);

            Console.WriteLine("success.....");
            Console.ReadKey();
        }
    }
}
