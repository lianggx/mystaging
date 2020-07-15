using MyStaging.Metadata;
using System.Collections.Generic;

namespace MyStaging.MySql
{
    public class MysqlType
    {
        private readonly static Dictionary<string, string> csharpTypes = new Dictionary<string, string> {
                { "char(36)", "Guid" },
                { "tinyint(1)", "bool" },
                { "tinyint", "sbyte" },
                { "smallint", "short"},
                { "integer", "int"},
                { "mediumint", "int"},
                { "int", "int"},
                { "bigint", "long"},
                { "bit", "ulong"},
                { "real","double"},
                { "double","double"},
                { "float", "float"},
                { "decimal", "decimal"},
                { "numeric", "decimal"},
                { "char", "string"},
                { "varchar", "string"},
                { "date", "DateTime"},
                { "time", "TimeSpan"},
                { "year", "DateTime"},
                { "timestamp", "DateTime"},
                { "datetime", "DateTime"},
                { "tinyblob", "byte[]"},
                { "blob", "byte[]" },
                { "mediumblob", "byte[]" },
                { "longblob", "byte[]" },
                { "tinytext", "string" },
                { "text", "string"},
                { "mediumtext", "string"},
                { "enum", "string"},
                { "binary", "byte[]"},
                 { "varbinary", "byte[]"},
                { "json", "JToken"}
        };
        private readonly static Dictionary<string, string> dbTypes = new Dictionary<string, string> {
                { "Guid","char(36)" },
                { "Int16", "smallint"},
                { "Int32", "int"},
                { "Int64", "bigint"},
                { "UInt16", "smallint"},
                { "UInt32", "int"},
                { "UInt64", "bigint"},
                { "Decimal", "decimal"},
                { "Double","double"},
                { "Single","float"},
                { "Boolean", "tinyint(1)" },
                { "Byte","bit" },
                { "SByte","tinyint" },
                { "Char","char" },
                { "String","varchar" },
                { "DateTimeOffset","datetime" },
                { "TimeSpan","time"},
                { "DateTime", "datetime"},
                { "JToken", "json"}
        };
        private readonly static Dictionary<string, string> contrastTypes = new Dictionary<string, string> {
                { "int", "int"},
                { "smallint", "short"},
                { "decimal", "decimal"},
                { "double","double"},
                { "float", "float"},
                { "bigint", "long"},
                { "char", "char"},
                { "varchar", "string"},
                { "binary", "byte[]" },
                { "bit", "byte" },
                { "timestamp", "DateTime" },
                { "datetime", "DateTime" },
                { "time", "TimeSpan"},
                { "json", "JToken"},
               { "tinyint", "sbyte" },
        };

        public static string SwitchToCSharp(string type)
        {
            if (csharpTypes.ContainsKey(type))
                return csharpTypes[type];
            else
                return type;
        }

        public static string ContrastType(string type)
        {
            foreach (var k in contrastTypes.Keys)
            {
                if (k == type)
                {
                    return contrastTypes[k];
                }
            }
            return null;
        }

        public static string GetDbType(string csType)
        {
            foreach (var k in dbTypes.Keys)
            {
                if (k == csType)
                {
                    return dbTypes[k];
                }
            }
            return null;
        }

        public static string GetRealType(DbFieldInfo fi)
        {
            var realType = fi.DbTypeFull ?? fi.DbType;
            return realType == "varchar" || realType == "char" ? realType + "(255)" : realType;
        }
    }
}
