using MyStaging.Gen.Tool;
using MyStaging.Gen.Tool.Models;
using System;
using System.Reflection;

namespace MyStaging.App
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args == null || args.Length == 0)
            //{
            //    Console.WriteLine("try --help");
            //    return;
            //}

            //if (args[0] == "--help")
            //{
            //    Console.WriteLine("Use the following parameters to create a project using Mystaging.App,The parameter name ignore case");
            //    Console.WriteLine("-h [host/ip] required");
            //    Console.WriteLine("-p [port]  required");
            //    Console.WriteLine("-u [database access username]  required");
            //    Console.WriteLine("-a [database auth password]  required");
            //    Console.WriteLine("-d [database]  required");
            //    Console.WriteLine("-pool [maxinum pool size numbric,default 32] optional");
            //    Console.WriteLine("-proj [the project build name]  required");
            //    Console.WriteLine("-0 [the project output path]  required");
            //    return;
            //}

            //string projName = string.Empty, outPutPath = string.Empty;
            //StringBuilder connection = new StringBuilder();
            //for (int i = 0; i < args.Length; i++)
            //{
            //    var item = args[i].ToLower();
            //    switch (item)
            //    {
            //        case "-h": connection.Append($"host={args[i + 1]};"); break;
            //        case "-p": connection.Append($"port={args[i + 1]};"); break;
            //        case "-u": connection.Append($"username={args[i + 1]};"); break;
            //        case "-a": connection.Append($"password={args[i + 1]};"); break;
            //        case "-d": connection.Append($"database={args[i + 1]};"); break;
            //        case "-pool": connection.Append($"maximum pool size={args[i + 1]};"); break;
            //        case "-proj": projName = args[i + 1]; break;
            //        case "-o": outPutPath = args[i + 1]; break;
            //    }
            //    i++;
            //}

            var config = new ProjectConfig
            {
                OutputDir = @"D:\MyGitHub\mystaging\examples\Pgsql",
                ProjectName = "Pgsql",
                ConnectionString = "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;Pooling=true;Maximum Pool Size=10;",
                Provider = "MyStaging.PostgreSQL"
            };

            //try
            //{
            IGeneralFactory factory = null;
            var types = Assembly.LoadFrom("MyStaging.PostgreSQL.dll").GetTypes();
            foreach (var t in types)
            {
                if (t.GetInterface(typeof(IGeneralFactory).Name) != null)
                {
                    factory = (IGeneralFactory)Activator.CreateInstance(t);
                }
            }

            factory.Build(config);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("{0}\n{1}",ex.Message,ex.StackTrace);
            //    throw ex;
            //}

            Console.WriteLine("success.....");
        }
    }
}


