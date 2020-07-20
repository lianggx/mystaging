using MyStaging.Metadata;
using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
using System.IO;
using System.Diagnostics;
using MyStaging.Interface;

namespace MyStaging.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Drawing();
            if (args.Length == 0)
                return;

            ProjectConfig config = GetConfig(args);
            if (config == null)
                return;

            try
            {
                IGeneralFactory factory = CreateGeneral(config);

                if (config.Mode == GeneralInfo.Db)
                    factory.DbFirst(config);
                else
                    factory.CodeFirst(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}\n{1}", ex.Message, ex.StackTrace);
            }

            Console.WriteLine("success.");
        }

        static void Drawing()
        {
            Console.WriteLine("欢迎使用 MyStaging.Gen");
            Console.WriteLine();
            Console.WriteLine("////////////////////////////////////////////////////////");
            Console.WriteLine("///                                                  ///");
            Console.WriteLine("///                        | |      (_)              ///");
            Console.WriteLine("///    _ __ ___  _   _ ___| |_ __ _ _ _ __   __ _    ///");
            Console.WriteLine(@"///   | '_ ` _ \| | | / __| __/ _` | | '_ \ / _` |   ///");
            Console.WriteLine(@"///   | | | | | | |_| \__ \ || (_| | | | | | (_| |   ///");
            Console.WriteLine(@"///   |_| |_| |_|\__, |___/\__\__,_|_|_| |_|\__, |   ///");
            Console.WriteLine("///               __/ |                      __/ |   ///");
            Console.WriteLine("///              |___/                      |___/    ///");
            Console.WriteLine("///                                                  ///");
            Console.WriteLine("////////////////////////////////////////////////////////");
            Console.WriteLine();

            Help();
            Console.WriteLine("查看帮助请使用命令 mystaging.gen --help");
            Console.WriteLine();
        }

        static void Help()
        {
            Console.WriteLine("要使用 MyStaging.Gen 请跟进下面的参数说明，执行创建实体对象映射.");
            Console.WriteLine();
            Console.WriteLine("--help 查看帮助");
            Console.WriteLine("-m [mode，db[DbFirst]/code[CodeFirst]，默认为 DbFirst");
            Console.WriteLine("-t [dbtype[Mysql/PostgreSQL]，数据库提供程序]  required");
            Console.WriteLine("-d [database，数据库连接字符串] required");
            Console.WriteLine("-p [project，项目名称]  required");
            Console.WriteLine("-o [output，实体对象输出路径]，默认为 {project}/Models");
            Console.WriteLine();
            Console.WriteLine("==============示例==============");
            Console.WriteLine("  CodeFirst：");
            Console.WriteLine("  mystaging.gen -m code -t PostgreSQL -p Pgsql -d \"Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;\"");
            Console.WriteLine();
            Console.WriteLine("  DbFirst：");
            Console.WriteLine("  mystaging.gen -m db -t PostgreSQL -p Pgsql -d \"Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;\"");
            Console.WriteLine("================================");
            Console.WriteLine();
        }

        static ProjectConfig GetConfig(string[] args)
        {
            if (args[0] == "--help")
            {
                Help();
                return null;
            }

            var config = new ProjectConfig();
            string mode = "db";
            for (int i = 0; i < args.Length; i++)
            {
                var item = args[i].ToLower();
                switch (item)
                {
                    case "-d": config.ConnectionString = args[i + 1]; break;
                    case "-p": config.ProjectName = args[i + 1]; break;
                    case "-o": config.OutputDir = args[i + 1]; break;
                    case "-t": config.Provider = args[i + 1]; break;
                    case "-m": mode = args[i + 1].ToLower(); break;
                }
                i++;
            }

            MyStaging.Common.CheckNotNull.NotEmpty(config.ConnectionString, "-d 参数必须提供");
            MyStaging.Common.CheckNotNull.NotEmpty(config.ProjectName, "-p 参数必须提供");
            MyStaging.Common.CheckNotNull.NotEmpty(config.Provider, "-t 参数必须提供");
            MyStaging.Common.CheckNotNull.NotEmpty(mode, "-m 参数必须提供");

            if (mode != "db" && mode != "code")
            {
                Console.WriteLine("-m 参数错误，必须为 db 或者 code");
                return null;
            }

            config.Mode = mode == "db" ? GeneralInfo.Db : GeneralInfo.Code;
            if (config.Mode == GeneralInfo.Db && string.IsNullOrEmpty(config.OutputDir))
            {
                config.OutputDir = Path.Combine(config.ProjectName, "Model");
            }

            return config;
        }

        static IGeneralFactory CreateGeneral(ProjectConfig config)
        {
            var fileName = "MyStaging." + config.Provider + ".dll";
            var dir = System.IO.Directory.GetCurrentDirectory();
            var providerFile = System.IO.Directory.GetFiles(dir, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(providerFile))
                throw new FileNotFoundException(fileName);

            IGeneralFactory factory = null;
            var types = Assembly.LoadFrom(providerFile).GetTypes();
            foreach (var t in types)
            {
                if (t.GetInterface(typeof(IGeneralFactory).Name) != null)
                {
                    factory = (IGeneralFactory)Activator.CreateInstance(t);
                    break;
                }
            }

            return factory;
        }
    }
}