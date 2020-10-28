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
using MyStaging.Common;

namespace MyStaging.App
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
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
            Console.WriteLine("OutputDir：{0}", config.OutputDir);
            Console.WriteLine("success.");
        }

        static void ShowHelp()
        {
            Console.WriteLine(@"欢迎使用 MyStaging.Gen，查看帮助请使用命令 mystaging.gen --help

////////////////////////////////////////////////////////
///                                                  ///
///                       | |      (_)               ///
///    _ __ ___  _   _ ___| |_ __ _ _ _ __   __ _    ///
///   | '_ ` _ \| | | / __| __/ _` | | '_ \ / _` |   ///
///   | | | | | | |_| \__ \ || (_| | | | | | (_| |   ///
///   |_| |_| |_|\__, |___/\__\__,_|_|_| |_|\__, |   ///
///               __/ |                      __/ |   ///
///              |___/                      |___/    ///
///                                                  ///
////////////////////////////////////////////////////////

要使用 MyStaging.Gen 请跟据下面的参数说明，执行创建实体对象映射.

--help 查看帮助
- m[mode，db[DbFirst] / code[CodeFirst]，默认为 DbFirst
- t[dbtype[Mysql / PostgreSQL]，数据库提供程序]  required
- d[database，数据库连接字符串] required
- n[name，数据库上下文名称]  required
- o[output，实体对象输出路径]，默认为 { name}/ Model

==============示例==============
  CodeFirst：
  mystaging.gen -m code -t Mysql -n Mysql -o Model -d ""server=127.0.0.1;port=3306;user id=root;password=root;database=mystaging;""
  DbFirst：
  mystaging.gen -m db -t Mysql -n Mysql -o Model -d ""server=127.0.0.1;port=3306;user id=root;password=root;database=mystaging;""
================================
");
        }

        static ProjectConfig GetConfig(string[] args)
        {
            if (args[0] == "--help")
            {
                ShowHelp();
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

            CheckNotNull.NotEmpty(config.ConnectionString, "-d 参数必须提供");
            CheckNotNull.NotEmpty(config.ContextName, "-n 参数必须提供");
            CheckNotNull.NotEmpty(config.Provider, "-t 参数必须提供");
            CheckNotNull.NotEmpty(mode, "-m 参数必须提供");

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
            CheckNotNull.NotNull(typeof(IGeneralFactory), $"程序集中 {providerAssembly.FullName} 找不到 IGeneralFactory 的实现。");
            return (IGeneralFactory)Activator.CreateInstance(type);
        }
    }
}