using MyStaging.Metadata;
using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
using System.IO;
using System.Diagnostics;
using MyStaging.Interface;
using System.Runtime.Loader;

namespace MyStaging.App
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Drawing();
                return;
            }

            ProjectConfig config = GetConfig(args);
            if (config == null)
                return;

            //ProjectConfig config = new ProjectConfig()
            //{
            //    ConnectionString = "Host=127.0.0.1;Port=3306;Username=root;Password=root;Database=mystaging;",
            //    Mode = GeneralInfo.Db,
            //    OutputDir = "Models",
            //    ContextName = "MyStaging",
            //    Provider = "MySql",
            //    ProviderAssembly = Assembly.LoadFile(@"D:\MyGitHub\mystaging\src\MyStaging.Gen\bin\Debug\netcoreapp3.1\MyStaging.MySql.dll")
            //};

            try
            {
                IGeneralFactory factory = CreateGeneral(config.ProviderAssembly);
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
            Console.WriteLine("-n [name，数据库上下文名称]  required");
            Console.WriteLine("-o [output，实体对象输出路径]，默认为 {name}/Models");
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
                Drawing();
                return null;
            }

            var config = new ProjectConfig();
            string mode = "db";
            for (int i = 0; i < args.Length; i++)
            {
                var item = args[i].ToLower();
                switch (item)
                {
                    case "-d":
                        config.ConnectionString = args[i + 1];
                        break;
                    case "-n": config.ContextName = args[i + 1]; break;
                    case "-o": config.OutputDir = args[i + 1]; break;
                    case "-t": config.Provider = args[i + 1]; break;
                    case "-m": mode = args[i + 1].ToLower(); break;
                }
                i++;
            }

            MyStaging.Common.CheckNotNull.NotEmpty(config.ConnectionString, "-d 参数必须提供");
            MyStaging.Common.CheckNotNull.NotEmpty(config.ContextName, "-n 参数必须提供");
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
                config.OutputDir = Path.Combine(config.ContextName, "Model");
            }

            var fileName = "MyStaging." + config.Provider;
            config.ProviderAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(fileName));

            // 生成批处理文件
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("mystaging.gen -m {0}", config.Mode)
                .AppendFormat(" -t {0}", config.Provider)
                .AppendFormat(" -n {0}", config.ContextName)
                .AppendFormat(" -d \"{0}\"", config.ConnectionString)
                .AppendFormat(" -o {0}", config.OutputDir);

            var buildFile = Path.Combine(config.ContextName, "build.bat");
            if (!Directory.Exists(config.ContextName))
            {
                Directory.CreateDirectory(config.ContextName);
            }

            File.WriteAllText(buildFile, sb.ToString(), Encoding.UTF8);

            return config;
        }

        static IGeneralFactory CreateGeneral(Assembly providerAssembly)
        {
            var type = providerAssembly.GetTypes().Where(f => f.GetInterface(typeof(IGeneralFactory).Name) != null).FirstOrDefault();
            MyStaging.Common.CheckNotNull.NotNull(typeof(IGeneralFactory), $"程序集中 {providerAssembly.FullName} 找不到 IGeneralFactory 的实现。");
            return (IGeneralFactory)Activator.CreateInstance(type);
        }
    }
}