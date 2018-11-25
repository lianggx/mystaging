using MyStaging.Helpers;
using System;
using System.Text;

namespace MyStaging.App
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("try --help");
                return;
            }

            if (args[0] == "--help")
            {
                Console.WriteLine("Use the following parameters to create a project using Mystaging.App,The parameter name ignore case");
                Console.WriteLine("-h [host/ip] required");
                Console.WriteLine("-p [port]  required");
                Console.WriteLine("-u [postgresql database access username]  required");
                Console.WriteLine("-a [postgresql database auth password]  required");
                Console.WriteLine("-d [postgresql database]  required");
                Console.WriteLine("-pool [maxinum pool size numbric,default 32] optional");
                Console.WriteLine("-proj [the project build name]  required");
                Console.WriteLine("-0 [the project output path]  required");
                return;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string projName = string.Empty, outPutPath = string.Empty;
            StringBuilder connection = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                var item = args[i].ToLower();
                switch (item)
                {
                    case "-h": connection.Append($"host={args[i + 1]};"); break;
                    case "-p": connection.Append($"port={args[i + 1]};"); break;
                    case "-u": connection.Append($"username={args[i + 1]};"); break;
                    case "-a": connection.Append($"password={args[i + 1]};"); break;
                    case "-d": connection.Append($"database={args[i + 1]};"); break;
                    case "-pool": connection.Append($"maximum pool size={args[i + 1]};"); break;
                    case "-proj": projName = args[i + 1]; break;
                    case "-o": outPutPath = args[i + 1]; break;
                }
                i++;
            }
            //outPutPath = @"E:\my\";
            //projName = "Crmmt";
            //PgSqlHelper.InitConnection(null, "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=crmmt;Pooling=true;Maximum Pool Size=100");
            PgSqlHelper.InitConnection(null, connection.ToString());
            GeneralFactory.Build(outPutPath, projName);

            Console.WriteLine("success.....");
        }
    }
}


