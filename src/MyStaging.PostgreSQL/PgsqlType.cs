using NpgsqlTypes;
using System.Collections.Generic;
using MyStaging.Common;

namespace MyStaging.PostgreSQL
{
    /// <summary>
    /// pgsql 数据库类型转换管理对象
    /// </summary>
    public class PgsqlType
    {
        private readonly static Dictionary<string, string> csharpTypes = new Dictionary<string, string> {
                { "uuid", "Guid" },
                { "oid", "uint"},
                { "xid", "uint"},
                { "cid", "uint"},
                { "integer", "int"},
                { "serial", "int"},
                { "serial4", "int"},
                { "int4", "int"},
                { "oidvector","uint[]" },
                { "serial2", "short"},
                { "smallint", "short"},
                { "int2", "short"},
                { "money", "decimal"},
                { "decimal", "decimal"},
                { "numeric", "decimal"},
                { "real", "decimal"},
                { "double","double"},
                { "float4","double"},
                { "float8","double"},
                { "int8", "long"},
                { "serial8", "long"},
                { "bigserial", "long"},
                { "name", "string"},
                { "varchar", "string"},
                { "char", "string"},
                { "bpchar", "string"},
                { "text", "string"},
                { "boolean", "bool" },
                { "bytea", "byte[]" },
                { "bit", "byte" },
                { "timetz", "DateTimeOffset" },
                { "time", "TimeSpan"},
                { "interval", "TimeSpan"},
                { "date", "DateTime"},
                { "timestamptz", "DateTime"},
                { "timestamp", "DateTime"},
                { "json", "JToken"},
                { "jsonb", "JToken"},
                { "geometry", "object"},
                { "path", "NpgsqlPath"},
                { "line", "NpgsqlLine"},
                { "polygon", "NpgsqlPolygon"},
                { "circle", "NpgsqlCircle"},
                { "point", "NpgsqlPoint"},
                { "box", "NpgsqlBox"},
                { "lseg", "NpgsqlLSeg"},
                { "inet", "System.Net.IPAddress"},
                { "macaddr", "System.Net.NetworkInformation.PhysicalAddress"},
                { "xml", "System.Xml.Linq.XDocument"},
                { "varbit", "System.Collections.BitArray"}
        };
        private readonly static Dictionary<string, NpgsqlDbType?> dbTypes = new Dictionary<string, NpgsqlDbType?> {
                { "e", null },
                { "int2", NpgsqlDbType.Integer},
                { "int4", NpgsqlDbType.Integer},
                { "int8", NpgsqlDbType.Bigint},
                { "bool", NpgsqlDbType.Boolean},
                { "bpchar", NpgsqlDbType.Char},
                { "float4", NpgsqlDbType.Double},
                { "float8", NpgsqlDbType.Double},
                { "interval",NpgsqlDbType.Interval },
                { "macaddr", NpgsqlDbType.MacAddr}
        };

        public static string SwitchToCSharp(string type)
        {
            if (csharpTypes.ContainsKey(type))
                return csharpTypes[type];
            else
                return type;
        }

        public static NpgsqlDbType? SwitchToDbType(string type)
        {
            if (dbTypes.ContainsKey(type))
            {
                return dbTypes[type];
            }
            else if (System.Enum.TryParse<NpgsqlDbType>(type.ToUpperPascal(), out NpgsqlDbType dbType))
                return dbType;

            return null;
        }
    }
}
