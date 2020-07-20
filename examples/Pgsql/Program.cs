using MyStaging.Interface;
using MyStaging.PostgreSQL.Generals;
using System;
using System.Threading;

namespace Pgsql
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            IGeneralFactory factory = new GeneralFactory();
            factory.CodeFirst(new MyStaging.Metadata.ProjectConfig()
            {
                ConnectionString = "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;",
                Mode = MyStaging.Metadata.GeneralInfo.Db,
                OutputDir = @"D:\MyGitHub\mystaging\examples\Pgsql\Models",
                ProjectName = "Pgsql",
                Provider = "PostgreSQL"
            });


            //PgsqlDbContext context = new PgsqlDbContext(new MyStaging.Common.StagingOptions("Pgsql", "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;"));

            //var udt3 = new Model.Udt3() { id = 1 };
            //context.Udt3.Insert.Add(udt3);
            // ct.Start();
            //Test();

            //   Console.WriteLine(GC.GetTotalMemory(false)/1024);
            // GC.Collect();
            //  Console.WriteLine(GC.GetTotalMemory(true)/1024);
            Console.ReadKey();
            Console.WriteLine("success.....");
        }

        static void Test()
        {
            //for (int i = 0; i < 5; i++)
            //{
            //    ContextTest ct = new ContextTest();
            //    var thread = new Thread(new ThreadStart(ct.Start));
            //    thread.IsBackground = true;
            //    thread.Start();
            //}
        }
    }
}


