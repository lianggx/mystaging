namespace MyStaging.xUnitTest
{
    public class ConstantUtil
    {
        public static string PGSQL_CONNECTION = "Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres;Database=mystaging;Pooling=true;Maximum Pool Size=1000;Timeout=10;CommandTimeout=120;";

        public static string MYSQL_CONNECTION = "server=127.0.0.1;user id=root;password=root;database=mystaging";

        public readonly static string REDIS_CONNECTION = "127.0.0.1:6379,defaultDatabase=1,name=mystaging,prefix=mystaging,abortConnect=false";
    }
}
