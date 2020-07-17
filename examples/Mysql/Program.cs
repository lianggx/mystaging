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

            var options = new MyStaging.Common.StagingOptions("MySql", "server=127.0.0.1;user id=root;password=root;database=mystaging");
            var context = new MysqlDbContext(options);

            var customer = new Customer { Name = "好久不见" };
            context.Customer.Insert.Add(customer);

            var article = context.Article.Select.InnerJoin<Customer>("b", (a, b) => a.userid == b.Id).Where<Customer>(f => f.Id == 2).ToOne();
            var a3 = context.Article.Update.SetValue(f => f.content, "未来已来，从这里开始").Where(f => f.id == 1).SaveChange();
            var newArticle = new Article()
            {
                content = "博客内容简单无聊",
                createtime = DateTime.Now,
                userid = customer.Id,
                IP = "127.0.0.1",
                State = true,
                title = "回顾与众不同的10年"
            };

            var list = new System.Collections.Generic.List<Article>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(newArticle);
            }
            //  var a2 = context.Article.Insert.Add(newArticle);
            var affrows = context.Article.Insert.AddRange(list).SaveChange();
            Console.WriteLine(affrows);
            // context.Article.Delete.Where(f => f.id == a2.id).SaveChange();

            //    Console.WriteLine(a2.id);

            Console.WriteLine("success.....");
            Console.ReadKey();
        }
    }
}
