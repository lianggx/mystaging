using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.xUnitTest
{
    public class ConstantUtil
    {
        /// <summary>
        ///  数据库连接字符串
        /// </summary>
        public static string PGSQL_CONNECTION = "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;Pooling=true;Maximum Pool Size=1000;Timeout=10;CommandTimeout=120;";

        public readonly static string REDIS_CONNECTION = "127.0.0.1:6379,defaultDatabase=1,name=mystaging,password=123456,prefix=mystaging,abortConnect=false";
    }
}
