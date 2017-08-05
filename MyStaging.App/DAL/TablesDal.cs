using MyStaging.App.Models;
using MyStaging.Common;
using MyStaging.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace MyStaging.App.DAL
{
    public class TablesDal
    {
        #region identity
        private string projectName = string.Empty;
        private string modelpath = string.Empty;
        private string dalpath = string.Empty;
        private string schemaName = string.Empty;
        private TableViewModel table = null;
        private List<FieldInfo> fieldList = new List<FieldInfo>();
        private List<PrimarykeyInfo> pkList = new List<PrimarykeyInfo>();
        private List<ConstraintInfo> consList = new List<ConstraintInfo>();
        #endregion

        public TablesDal(string projectName, string modelpath, string dalpath, string schemaName, TableViewModel table)
        {
            this.projectName = projectName;
            this.modelpath = modelpath;
            this.dalpath = dalpath;
            this.schemaName = schemaName;
            this.table = table;
            Get_Fields();
            Get_Primarykey(fieldList[0].Oid);
            Get_Constraint();
        }

        public void Generate()
        {
            string _classname = $"{this.schemaName.ToUpperPascal()}_{this.table.name}Model";
            string _fileName = $"{modelpath}/{_classname}.cs";
            using (StreamWriter writer = new StreamWriter(File.Create(_fileName)))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine($"using {projectName}.DAL;");
                writer.WriteLine("using Newtonsoft.Json;");
                writer.WriteLine("using Newtonsoft.Json.Linq;");
                writer.WriteLine("using MyStaging.Mapping;");
                writer.WriteLine();
                writer.WriteLine($"namespace {projectName}.Model");
                writer.WriteLine("{");
                writer.WriteLine($"\t[EntityMapping(TableName = \"{this.schemaName}.{this.table.name}\")]");
                writer.WriteLine($"\tpublic partial class {_classname}");
                writer.WriteLine("\t{");

                foreach (var item in fieldList)
                {
                    if (!string.IsNullOrEmpty(item.Comment))
                    {
                        writer.WriteLine("\t\t/// <summary>");
                        writer.WriteLine($"\t\t/// {item.Comment}");
                        writer.WriteLine("\t\t/// </summary>");
                    }

                    writer.WriteLine($"\t\tpublic {item.RelType} {item.Field.ToUpperPascal()} {{ get;set; }}");
                }
                Hashtable ht = new Hashtable();
                foreach (var item in consList)
                {
                    string pname = $"{item.table_name.ToUpperPascal()}";
                    string propertyName = $"{item.nspname.ToUpperPascal()}_{pname}";
                    if (ht.ContainsKey(propertyName))
                    {
                        propertyName += "By" + item.conname;
                    }
                    string dalName = $"{item.nspname.ToUpperPascal()}_{item.table_name}";
                    writer.WriteLine($"\t\t[ForeignKeyMapping]public {dalName}Model {propertyName} {{ get{{ return {dalName}.Context.Where(f=>f.{item.ref_column.ToUpperPascal()}==this.{item.conname.ToUpperPascal()}).ToOne(); }} }}");
                    ht.Add(propertyName, "");
                }

                writer.WriteLine("\t}");
                writer.WriteLine("}");
                writer.Flush();

                CreateDal();
            }
        }

        protected void CreateDal()
        {
            string _model_classname = $"{this.schemaName.ToUpperPascal()}_{this.table.name}Model";
            string _classname = $"{this.schemaName.ToUpperPascal()}_{this.table.name}";
            string _fileName = $"{dalpath}/{_classname}.cs";
            using (StreamWriter writer = new StreamWriter(File.Create(_fileName)))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using Newtonsoft.Json;");
                writer.WriteLine("using Newtonsoft.Json.Linq;");
                writer.WriteLine("using MyStaging;");
                writer.WriteLine("using MyStaging.Helpers;");
                writer.WriteLine("using MyStaging.Common;");
                writer.WriteLine("using NpgsqlTypes;");
                writer.WriteLine($"using {projectName}.Model;");
                writer.WriteLine();
                writer.WriteLine($"namespace {projectName}.DAL");
                writer.WriteLine("{");
                writer.WriteLine($"\tpublic partial class {_classname}: QueryContext<{_model_classname}>");
                writer.WriteLine("\t{");

                StringBuilder sb_field = new StringBuilder();
                StringBuilder sb_param = new StringBuilder();

                for (int i = 0; i < fieldList.Count; i++)
                {
                    var item = fieldList[i];
                    if (item.Is_identity) continue;
                    sb_field.Append($"{item.Field}");
                    sb_param.Append($"@{item.Field}");
                    if (fieldList.Count > i + 1)
                    {
                        sb_field.Append(",");
                        sb_param.Append(",");
                    }
                }

                string insert_sql = $"INSERT INTO \\\"{this.schemaName}\\\".\\\"{this.table.name}\\\"({sb_field.ToString()}) VALUES({sb_param.ToString()}) RETURNING {sb_field.ToString()};";
                writer.WriteLine($"\t\tconst string insertCmdText=\"{insert_sql}\";");

                StringBuilder sb_primarykey = new StringBuilder();
                for (int i = 0; i < pkList.Count; i++)
                {
                    var item = pkList[i];
                    sb_primarykey.Append($"{item.Field}=@{item.Field}");
                    if (pkList.Count > i + 1)
                        sb_primarykey.Append(" and ");
                }
                string pkString = sb_primarykey.ToString();

                writer.WriteLine($"\t\tconst string deleteCmdText=\"DELETE FROM \\\"{this.schemaName}\\\".\\\"{this.table.name}\\\" WHERE {pkString}\";");
                writer.WriteLine($"\t\tpublic static {_classname} Context{{get{{return new {_classname}();}}}}");

                if (this.table.type == "table")
                {
                    writer.WriteLine();
                    Insert_Generator(writer, fieldList, _model_classname, _classname);
                    writer.WriteLine();
                    Delete_Generator(writer, _model_classname, _classname);
                    writer.WriteLine();
                    Update_Generator(writer, _model_classname, _classname);
                    writer.WriteLine();
                }

                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }

        protected void Insert_Generator(StreamWriter writer, List<FieldInfo> fieldList, string class_model, string className)
        {
            writer.WriteLine($"\t\tpublic static {class_model} Insert({class_model} model)");
            writer.WriteLine("\t\t{");
            string _cn = className.ToLower();
            writer.WriteLine($"\t\t\t{className} {_cn} = Context;");
            foreach (var item in fieldList)
            {
                if (item.Is_identity) continue;
                string _dbtype = PgsqlType.SwitchToSql(item.Data_Type, item.Db_type);
                string specificType = GetspecificType(item);
                writer.WriteLine($"\t\t\t{_cn}.AddParameter(\"{ item.Field}\", NpgsqlDbType.{_dbtype}, model.{item.Field.ToUpperPascal()},{specificType});");
            }
            writer.WriteLine();
            writer.WriteLine($"\t\t\treturn {_cn}.InsertOnReader(insertCmdText);");
            writer.WriteLine("\t\t}");
        }

        protected void Update_Generator(StreamWriter writer, string class_model, string dal_name)
        {
            List<string> d_key = new List<string>();
            List<string> d_key_fields = new List<string>();
            foreach (var item in pkList)
            {
                FieldInfo fs = fieldList.FirstOrDefault(f => f.Field == item.Field);
                d_key.Add(fs.RelType + " " + fs.Field);
                d_key_fields.Add(fs.Field);
            }
            string updateName = $"{this.table.name.ToUpperPascal()}UpdateBuilder";

            writer.WriteLine($"\t\tpublic static {updateName} Update({string.Join(",", d_key)})");
            writer.WriteLine("\t\t{");
            writer.WriteLine($"\t\t\t return new {updateName}({string.Join(",", d_key_fields)});");
            writer.WriteLine("\t\t}");
            writer.WriteLine();

            writer.WriteLine($"\t\tpublic class {updateName}:UpdateBuilder<{class_model.ToUpperPascal()}>");
            writer.WriteLine("\t\t{");
            writer.WriteLine($"\t\t\tpublic {updateName}({string.Join(",", d_key)})");
            writer.WriteLine("\t\t\t{");
            foreach (var item in pkList)
            {
                FieldInfo fi = fieldList.FirstOrDefault(f => f.Field == item.Field);
                string _dbtype = PgsqlType.SwitchToSql(fi.Data_Type, fi.Db_type);
                string specificType = GetspecificType(fi);
                writer.WriteLine($"\t\t\t\tbase.AddParameter(\"{fi.Field}\", NpgsqlDbType.{_dbtype}, {fi.Field},{specificType});");
            }
            writer.WriteLine("\t\t\t}");
            foreach (var item in fieldList)
            {
                if (item.Is_identity) continue;
                string _dbtype = PgsqlType.SwitchToSql(item.Data_Type, item.Db_type);

                writer.WriteLine($"\t\t\tpublic {updateName} Set{item.Field.ToUpperPascal()}({item.RelType} {item.Field})");
                writer.WriteLine("\t\t\t{");
                string specificType = GetspecificType(item);
                writer.WriteLine($"\t\t\t\treturn base.SetField(\"{ item.Field}\", NpgsqlDbType.{_dbtype}, {item.Field},{specificType}) as {updateName};");
                writer.WriteLine("\t\t\t}");
            }
            writer.WriteLine("\t\t}");
        }

        protected void Delete_Generator(StreamWriter writer, string class_model, string className)
        {
            List<string> d_key = new List<string>();
            foreach (var item in pkList)
            {
                FieldInfo fs = fieldList.FirstOrDefault(f => f.Field == item.Field);
                d_key.Add(fs.RelType + " " + fs.Field);
            }

            writer.WriteLine($"\t\tpublic static int Delete({string.Join(",", d_key)})");
            writer.WriteLine("\t\t{");
            string _cn = className.ToLower();
            writer.WriteLine($"\t\t\t{className} {_cn} = Context;");
            int len = pkList.Count;
            foreach (var item in pkList)
            {
                FieldInfo fi = fieldList.FirstOrDefault(f => f.Field == item.Field);
                string _dbtype = PgsqlType.SwitchToSql(fi.Data_Type, fi.Db_type);
                string specificType = GetspecificType(fi);
                writer.WriteLine($"\t\t\t{_cn}.AddParameter(\"{ item.Field}\", NpgsqlDbType.{_dbtype}, {item.Field},{specificType});");
            }
            writer.WriteLine($"\t\t\treturn {_cn}.ExecuteNonQuery(deleteCmdText);");
            writer.WriteLine("\t\t}");
        }

        #region primary key / constraint
        private void Get_Fields()
        {

            string _sqltext = @"SELECT a.oid
,c.attnum as num
,c.attname as field
,c.attlen as length
,c.atttypmod as lengthvar
,c.attnotnull as notnull
,d.description as comment
,(case when e.typelem = 0 then e.typname else e2.typname end) as type
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
            _sqltext = string.Format(_sqltext, this.schemaName, this.table.name);


            PgSqlHelper.ExecuteDataReader(dr =>
            {
                int f_oid = Convert.ToInt32(dr["oid"]);
                string field = dr["field"].ToString();
                string length = dr["length"].ToString();
                int lengthvar = Convert.ToInt32(dr["lengthvar"]);
                bool notnull = Convert.ToBoolean(dr["notnull"]);
                string comment = dr["comment"].ToString();
                string type = dr["type"].ToString();
                string data_type = dr["data_type"].ToString();
                string is_identity = dr["is_identity"].ToString();
                string typcategory = dr["typcategory"].ToString();
                string _type = PgsqlType.SwitchToCSharp(type);

                bool is_array = typcategory == "A";
                string _array = is_array ? "[]" : "";
                bool is_enum = data_type == "e";
                if (is_enum)
                    _type = _type.ToUpperPascal();

                string _notnull = "";
                if (_type != "string" && _type != "JToken" && !is_array)
                {
                    _notnull = notnull ? "" : "?";
                }

                string reltype = $"{_type}{_notnull}{_array}";
                // dal
                this.fieldList.Add(new FieldInfo()
                {
                    Field = field,
                    Comment = comment,
                    RelType = reltype,
                    Db_type = type,
                    Data_Type = data_type,
                    Is_identity = is_identity == "YES",
                    Is_array = data_type == "ARRAY",
                    Is_not_null = notnull,
                    Is_enum = is_enum,
                    Oid = f_oid
                });
            }, CommandType.Text, _sqltext);
        }

        private string GetspecificType(FieldInfo fi)
        {
            string specificType = "null";
            if (fi.Data_Type == "e")
                specificType = $"typeof({fi.RelType.Replace("?", "")})";

            return specificType;
        }

        protected void Get_Primarykey(int oid)
        {
            string _sqltext = $@"SELECT b.attname, format_type(b.atttypid, b.atttypmod) AS data_type
FROM pg_index a
INNER JOIN pg_attribute b ON b.attrelid = a.indrelid AND b.attnum = ANY(a.indkey)
WHERE a.indrelid = '{schemaName}.{table.name}'::regclass AND a.indisprimary;
";
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                PrimarykeyInfo pk = new PrimarykeyInfo();
                pk.Field = dr["attname"].ToString();
                pk.Typname = dr["data_type"].ToString();
                pkList.Add(pk);
            }, CommandType.Text, _sqltext);
        }

        protected void Get_Constraint()
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
WHERE b.nspname='{0}' and a.relname='{1}'
);", this.schemaName, this.table.name);


            PgSqlHelper.ExecuteDataReader(dr =>
                {
                    string conname = dr["conname"].ToString();
                    string contype = dr["typname"].ToString();
                    string ref_column = dr["ref_column"].ToString();
                    string relname = dr["relname"].ToString();
                    string nspname = dr["nspname"].ToString();
                    consList.Add(new ConstraintInfo()
                    {
                        conname = conname,
                        contype = contype,
                        ref_column = ref_column,
                        table_name = relname,
                        nspname = nspname
                    });
                }, CommandType.Text, _sqltext);
        }
        #endregion
    }
}
