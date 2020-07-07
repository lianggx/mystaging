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
            IGeneralFactory factory = new GeneralFactory();
            factory.DbFirst(new MyStaging.Metadata.ProjectConfig()
            {
                ConnectionString = "server=127.0.0.1;user id=root;password=root;database=mystaging",
                Mode = MyStaging.Metadata.GeneralMode.Db,
                OutputDir = @"D:\MyGitHub\mystaging\examples\Mysql\Models",
                ProjectName = "Mysql",
                Provider = "MySql"
            });

            //var options = new MyStaging.Common.StagingOptions("MySql", "server=127.0.0.1;user id=root;password=root;database=mystaging");
            //var context = new MysqlDbContext(options);

            //var aid = Guid.Parse("836c84d3-7fdf-42a6-8f26-f694c324fb63");
            //var article = context.Article.Select.Where(f => f.AID == aid).ToOne();

            //var content = new JObject();
            //content["title"] = "未来已来，从这里开始";
            //context.Article.Update.SetValue(f => f.content, content.ToString()).Where(f => f.AID == aid).SaveChange();


            Console.WriteLine("Hello MySql");
            Console.ReadKey();
        }
    }
}
