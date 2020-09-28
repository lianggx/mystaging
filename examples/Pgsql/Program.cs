using MyStaging.Metadata;
using System;

namespace Pgsql
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            PgsqlDbContext dbContext = new PgsqlDbContext(new StagingOptions("Pgsql", "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;"));
            var art = dbContext.Article.Select.Where(f => f.id == "5ee1c721b5ee483998000001").ToOne();

            new ContextTest().Start();

            Console.ReadKey();
            Console.WriteLine("success.....");
        }


    }
}


