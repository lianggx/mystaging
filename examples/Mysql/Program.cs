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
            var article = context.Article.Select.Where(f => f.id == 1).ToOne();
            var content = "未来已来，从这里开始";
            var a3 = context.Article.Update.SetValue(f => f.content, content.ToString()).Where(f => f.id == 1).SaveChange();
            var newArticle = new Article()
            {
                age = 18,
                AID = Guid.NewGuid(),
                content = "博客内容简单无聊",
                createtime = DateTime.Now,
                // id = 1,
                IP = "127.0.0.1",
                money = 4000,
                State = true,
                title = "回顾与众不同的10年",
                total = 1,
                userid = ObjectId.NewId().ToString()
            };
            var a2 = context.Article.Insert.Add(newArticle);

            Console.WriteLine("success.....");
            Console.ReadKey();
        }
    }
}
