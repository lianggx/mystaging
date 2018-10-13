using NpgsqlTypes;
using System.Collections.Generic;

namespace MyStaging.Common
{
    /// <summary>
    /// pgsql 数据库类型转换管理对象
    /// </summary>
    public class PgsqlType
    {
        /// <summary>
        ///  转换数据库类型到C#
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string SwitchToCSharp(string type)
        {
            switch (type)
            {
                case "uuid": return "Guid";
                case "oid":
                case "xid":
                case "cid": return "uint";
                case "integer":
                case "serial":
                case "serial4":
                case "int4": return "int";
                case "oidvector": return "uint[]";
                case "serial2":
                case "smallint":
                case "int2": return "short";
                case "money":
                case "decimal":
                case "numeric":
                case "real": return "decimal";
                case "double":
                case "float4":
                case "float8": return "double";
                case "int8":
                case "serial8":
                case "bigserial": return "long";
                case "name":
                case "varchar":
                case "char":
                case "bpchar":
                case "text": return "string";
                case "boolean": return "bool";
                case "bytea": return "byte[]";
                case "bit": return "byte";
                case "timetz": return "DateTimeOffset";
                case "time":
                case "interval": return "TimeSpan";
                case "date":
                case "timestamptz":
                case "timestamp": return "DateTime";
                case "json": return "JToken";
                case "jsonb": return "JToken";
                case "geometry": return "object";
                case "path": return "NpgsqlPath";
                case "line": return "NpgsqlLine";
                case "polygon": return "NpgsqlPolygon";
                case "circle": return "NpgsqlCircle";
                case "point": return "NpgsqlPoint";
                case "box": return "NpgsqlBox";
                case "lseg": return "NpgsqlLSeg";
                case "inet": return "System.Net.IPAddress";
                case "macaddr": return "System.Net.NetworkInformation.PhysicalAddress";
                case "xml": return "System.Xml.Linq.XDocument";
                case "varbit": return "System.Collections.BitArray";

                default: return type;
            }
        }

        /// <summary>
        ///  转换数据库类型描述为 NpgsqlDbType 类型对象
        /// </summary>
        /// <param name="data_type"></param>
        /// <param name="db_type"></param>
        /// <returns></returns>
        public static NpgsqlDbType SwitchToSql(string data_type, string db_type)
        {

            NpgsqlDbType _dbtype;
            if (data_type == "e")
                _dbtype = NpgsqlDbType.Enum;  //   _dbtype = item.Db_type.ToUpperPascal();
            else if (db_type == "int2" || db_type == "int4")
            {
                _dbtype = NpgsqlDbType.Integer;
            }
            else if (db_type == "int8")
            {
                _dbtype = NpgsqlDbType.Bigint;
            }
            else if (db_type == "bool")
            {
                _dbtype = NpgsqlDbType.Boolean;
            }
            else if (db_type == "bpchar")
            {
                _dbtype = NpgsqlDbType.Char;
            }
            else if (db_type == "float4" || db_type == "float8")
            {
                _dbtype = NpgsqlDbType.Double;
            }
            else if (db_type == "path" || db_type == "line" || db_type == "polygon" || db_type == "circle" || db_type == "point" || db_type == "box" || db_type == "lseg")
            {
                if (db_type == "lseg") db_type = "LSeg";
                System.Enum.TryParse<NpgsqlDbType>(db_type.ToUpperPascal(), out _dbtype);
            }
            else if (db_type == "interval")
            {
                _dbtype = NpgsqlDbType.Interval;
            }
            else if (db_type == "macaddr")
            {
                _dbtype = NpgsqlDbType.MacAddr;
            }
            else
            {
                System.Enum.TryParse<NpgsqlDbType>(db_type.ToUpperPascal(), out _dbtype);
            }

            return _dbtype;
        }
    }
}
