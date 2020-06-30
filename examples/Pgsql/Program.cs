using MyStaging.Common;
using MyStaging.PostgreSQL;
using Newtonsoft.Json.Linq;
using Pgsql.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pgsql
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            // ct.Start();
            Test();

            //   Console.WriteLine(GC.GetTotalMemory(false)/1024);
            // GC.Collect();
            //  Console.WriteLine(GC.GetTotalMemory(true)/1024);
            Console.ReadKey();
            Console.WriteLine("success.....");
        }

        static void Test()
        {
            for (int i = 0; i < 5; i++)
            {
                ContextTest ct = new ContextTest();
                var thread = new Thread(new ThreadStart(ct.Start));
                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
}


