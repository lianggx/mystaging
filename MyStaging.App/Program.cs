using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Linq.Expressions;
using MyStaging.Common;
using MyStaging.Helpers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MyStaging;
//using gmall.Model;
//using gmall.DAL;
using System.IO.Compression;

namespace MyStaging.App
{
    class Program
    {
        static void Main(string[] args)
        {
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

            Console.WriteLine("已完成.....");
        }
    }
}





