using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Gen.Tool.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace MyStaging.PostgreSQL.Generals
{
    public class EntityGeneral
    {
        #region identity
        private readonly DbContext dbContext;
        public readonly TableInfo table;
        private readonly GeneralConfig config;
        private readonly List<FieldInfo> fields = new List<FieldInfo>();
        private readonly List<PrimarykeyInfo> primarykeys = new List<PrimarykeyInfo>();
        private readonly List<ConstraintInfo> constraints = new List<ConstraintInfo>();
        #endregion

        public EntityGeneral(DbContext dbContext, GeneralConfig config, TableInfo table)
        {
            this.dbContext = dbContext;
            this.config = config;
            this.table = table;
        }

        public void Create()
        {
            GetFields();
            GetPrimarykey();
            GetConstraint();
            CreateModel();
        }

        public void CreateModel()
        {
            string _classname = CreateName() + "Model";
            string _fileName = $"{config.ModelPath}/{_classname}.cs";
            using StreamWriter writer = new StreamWriter(File.Create(_fileName), System.Text.Encoding.UTF8);
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Linq;");
            writer.WriteLine("using Newtonsoft.Json;");
            writer.WriteLine("using Newtonsoft.Json.Linq;");
            writer.WriteLine("using MyStaging.Mapping;");
            writer.WriteLine("using NpgsqlTypes;");
            writer.WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
            writer.WriteLine();
            writer.WriteLine($"namespace {config.ProjectName}.Model");
            writer.WriteLine("{");
            writer.WriteLine($"\t[Table(name: \"{this.table.Name}\", Schema = \"{table.Schema}\")]");
            writer.WriteLine($"\tpublic partial class {_classname}");
            writer.WriteLine("\t{");

            foreach (var item in fields)
            {
                if (!string.IsNullOrEmpty(item.Comment))
                {
                    writer.WriteLine("\t\t/// <summary>");
                    writer.WriteLine($"\t\t/// {item.Comment}");
                    writer.WriteLine("\t\t/// </summary>");
                }
                if (primarykeys.Count(f => f.Field == item.Field) > 0)
                    writer.WriteLine($"\t\t[PrimaryKey]");

                string _type = item.RelType == "char" || item.RelType == "char?" ? "string" : item.RelType;
                writer.WriteLine($"\t\tpublic {_type} {item.Field} {{ get; set; }}");
            }
            writer.WriteLine("\t}");
            writer.WriteLine("}");
            writer.Flush();
        }

        private string CreateName()
        {
            return CreateName(table.Schema, this.table.Name);
        }

        private string CreateName(string schema, string tableName, string separator = "")
        {
            string _classname;
            if (schema == "public")
            {
                _classname = separator + tableName.ToUpperPascal();
            }
            else
            {
                _classname = $"{schema.ToUpperPascal()}{separator}{tableName.ToUpperPascal()}";
            }

            return _classname;
        }

        #region primary key / constraint
        private void GetFields()
        {

            string _sqltext = @"SELECT a.oid
,c.attnum as num
,c.attname as field
, (case when f.character_maximum_length is null then c.attlen else f.character_maximum_length end) as length
,c.attnotnull as notnull
,d.description as comment
,(case when e.typcategory ='G' then e.typname when e.typelem = 0 then e.typname else e2.typname end) as type
,(case when e.typelem = 0 then e.typtype else e2.typtype end) as data_type
,e.typcategory
,f.is_identity
                                from  pg_class a 
                                inner join pg_namespace b on a.relnamespace=b.oid
                                inner join pg_attribute c on attrelid = a.oid
                                LEFT OUTER JOIN pg_description d ON c.attrelid = d.objoid AND c.attnum = d.objsubid and c.attnum > 0
                                inner join pg_type e on e.oid=c.atttypid
                                left join pg_type e2 on e2.oid=e.typelem
                                inner join information_schema.columns f on f.table_schema = b.nspname and f.table_name=a.relname and column_name = c.attname
                                WHERE b.nspname='{0}' and a.relname='{1}';";
            _sqltext = string.Format(_sqltext, table.Schema, table.Name);

            dbContext.Execute.ExecuteDataReader(dr =>
            {
                FieldInfo fi = new FieldInfo
                {
                    Oid = Convert.ToInt32(dr["oid"]),
                    Field = dr["field"].ToString(),
                    Length = Convert.ToInt32(dr["length"].ToString()),
                    NotNull = Convert.ToBoolean(dr["notnull"]),
                    Comment = dr["comment"].ToString(),
                    DataType = dr["data_type"].ToString(),
                    DbType = dr["type"].ToString()
                };
                fi.DbType = fi.DbType.StartsWith("_") ? fi.DbType.Remove(0, 1) : fi.DbType;
                fi.Identity = dr["is_identity"].ToString() == "YES";
                fi.IsArray = dr["typcategory"].ToString() == "A";
                fi.IsEnum = fi.DataType == "e";

                fi.CsType = PgsqlType.SwitchToCSharp(fi.DbType);

                if (fi.IsEnum) fi.CsType = fi.CsType.ToUpperPascal();
                string _notnull = "";
                if (
                fi.CsType != "string"
                && fi.CsType != "byte[]"
                && fi.CsType != "JToken"
                && !fi.IsArray
                && fi.CsType != "System.Net.IPAddress"
                && fi.CsType != "System.Net.NetworkInformation.PhysicalAddress"
                && fi.CsType != "System.Xml.Linq.XDocument"
                && fi.CsType != "System.Collections.BitArray"
                && fi.CsType != "object"
                )
                    _notnull = fi.NotNull ? "" : "?";

                string _array = fi.IsArray ? "[]" : "";
                fi.RelType = $"{fi.CsType}{_notnull}{_array}";
                // dal
                this.fields.Add(fi);
            }, CommandType.Text, _sqltext);
        }

        protected void GetPrimarykey()
        {
            string _sqltext = $@"SELECT b.attname, format_type(b.atttypid, b.atttypmod) AS data_type
FROM pg_index a
INNER JOIN pg_attribute b ON b.attrelid = a.indrelid AND b.attnum = ANY(a.indkey)
WHERE a.indrelid = '{table.Schema}.{table.Name}'::regclass AND a.indisprimary;
";
            dbContext.Execute.ExecuteDataReader(dr =>
            {
                PrimarykeyInfo pk = new PrimarykeyInfo
                {
                    Field = dr["attname"].ToString(),
                    TypeName = dr["data_type"].ToString()
                };
                primarykeys.Add(pk);
            }, CommandType.Text, _sqltext);
        }

        protected void GetConstraint()
        {
            string _sqltext = string.Format(@"
SELECT(select attname from pg_attribute where attrelid = a.conrelid and attnum = any(a.conkey)) as conname
,b.relname,c.nspname,d.attname as ref_column,e.typname
FROM pg_constraint a 
left JOIN  pg_class b on b.oid= a.confrelid
inner join pg_namespace c on b.relnamespace = c.oid
INNER JOIN pg_attribute d on d.attrelid =a.confrelid and d.attnum=any(a.confkey)
inner join pg_type e on e.oid = d.atttypid
WHERE conrelid in 
(
SELECT a.oid FROM pg_class a 
inner join pg_namespace b on a.relnamespace=b.oid
WHERE b.nspname='{0}' and a.relname='{1}');"
        , table.Schema, this.table.Name);

            dbContext.Execute.ExecuteDataReader(dr =>
                {
                    string conname = dr["conname"].ToString();
                    string contype = dr["typname"].ToString();
                    string ref_column = dr["ref_column"].ToString();
                    string relname = dr["relname"].ToString();
                    string nspname = dr["nspname"].ToString();
                    constraints.Add(new ConstraintInfo()
                    {
                        ConlumnName = conname,
                        ConlumnType = contype,
                        RefColumn = ref_column,
                        TablaName = relname,
                        NspName = nspname
                    });
                }, CommandType.Text, _sqltext);
        }
        #endregion
    }
}
