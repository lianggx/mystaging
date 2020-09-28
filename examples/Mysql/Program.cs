using Mysql.Model;
using Mysql.Services;
using MyStaging.Common;
using MyStaging.Interface;
using MyStaging.Metadata;
using MyStaging.MySql.Generals;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mysql
{
    public class Program
    {
        private static MysqlDbContext dbContext;
        static void Main(string[] args)
        {
            var options = new StagingOptions("MySql", "server=127.0.0.1;user id=root;password=root;");
            dbContext = new MysqlDbContext(options);

            AddUpdateDelete();
            Query();
            Transaction();
            Console.WriteLine("success.....");
            Console.ReadKey();
        }

        static void Transaction()
        {
            var customer = new Customer { Name = "好久不见" };

            try
            {
                // 测试事务
                dbContext.BeginTransaction();
                dbContext.Customer.Insert.Add(customer);
                List<Customer> li = new List<Customer>();
                li.Add(new Customer { Name = "test" });
                dbContext.Customer.Insert.AddRange(li).SaveChange();
                dbContext.Customer.Update.SetValue(a => a.Name, "12345").Where(f => f.Id == customer.Id).SaveChange();
                dbContext.CommitTransaction();

                ArticleService articleService = new ArticleService(dbContext);
                var art = articleService.Detail(29);
                art = articleService.Update(art.id, "修改了标题", art.content);
                bool success = articleService.Delete(art.id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Query()
        {
            // 单个查询
            var article = dbContext.Customer.Select.Where(f => f.Id == 2 && f.Name == "Ron").ToOne();
            // 列表查询，排序、分页、分组
            var articles = dbContext.Customer.Select.OrderBy(f => f.Name).Page(1, 10).GroupBy("Id,Name").ToList<(int id, string name)>("Id,Name");
            // 表连接查询
            var ac = dbContext.Article.Select.InnerJoin<Customer>("b", (a, b) => a.userid == b.Id).Where<Customer>(f => f.Id == 2).ToOne();
            // 首字段查询，ToScalar 参数可以传递 Sql 参数，比如 SUM(x)
            var id = dbContext.Customer.Select.Where(f => f.Id == 2 && f.Name == "Ron").ToScalar<int>("Id");
        }

        static void AddUpdateDelete()
        {
            var art = new Article()
            {
                content = "你是谁？你从哪里来？要到哪里去？",
                createtime = DateTime.Now,
                userid = 43,
                IP = "127.0.0.1",
                State = true,
                title = "振聋发聩的人生三问"
            };

            var articles = new List<Article>();
            for (int i = 0; i < 10; i++)
            {
                articles.Add(art);
            }
            var a2 = dbContext.Article.Insert.Add(art);
            var affrows = dbContext.Article.Insert.AddRange(articles).SaveChange();

            var a3 = dbContext.Article.Update.SetValue(f => f.content, "未来已来，从这里开始").Where(f => f.id == 1).SaveChange();
            var a4 = dbContext.Article.Select.OrderByDescing(f => f.createtime).ToOne();
            dbContext.Article.Delete.Where(f => f.id == a4.id).SaveChange();
        }
    }
}
