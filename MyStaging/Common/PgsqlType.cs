using NpgsqlTypes;

namespace MyStaging.Common
{
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
                case "timestamp": return "DateTime";
                case "smallint": return "short";
                case "integer": return "int";
                case "int2": return "short";
                case "int4": return "int";
                case "int8": return "long";
                case "_int2": return "short";
                case "_int4": return "int";
                case "_int8": return "long";
                case "decimal": return "decimal";
                case "numeric": return "decimal";
                case "real": return "decimal";
                case "double": return "double";
                case "serial": return "int";
                case "bigserial": return "long";
                case "varchar": return "string";
                case "char": return "char";
                case "text": return "string";
                case "boolean": return "bool";
                case "bit": return "byte";
                case "_uuid": return "Guid";
                case "_timestamp": return "DateTime";
                case "_smallint": return "short";
                case "_integer": return "int";
                case "_bigint": return "long";
                case "_decimal": return "decimal";
                case "_numeric": return "decimal";
                case "_real": return "decimal";
                case "_double": return "double";
                case "_serial": return "int";
                case "_bigserial": return "long";
                case "_varchar": return "string";
                case "_char": return "char";
                case "_text": return "string";
                case "_boolean": return "bool";
                case "_bit": return "byte";
                case "json": return "JToken";
                case "jsonb": return "JToken";
                case "date": return "DateTime";
                default: return type;
            }
        }

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
            else
            {
                _dbtype = System.Enum.Parse<NpgsqlDbType>(db_type.ToUpperPascal());
            }

            return _dbtype;
        }
    }
}
