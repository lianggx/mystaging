using MyStaging.App.Models;
using MyStaging.Common;
using MyStaging.Helpers;
using NpgsqlTypes;
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
            string _classname = CreateName() + "Model";

            string _fileName = $"{modelpath}/{_classname}.cs";
            using (StreamWriter writer = new StreamWriter(File.Create(_fileName), System.Text.Encoding.UTF8))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine($"using {projectName}.DAL;");
                writer.WriteLine("using Newtonsoft.Json;");
                writer.WriteLine("using Newtonsoft.Json.Linq;");
                writer.WriteLine("using MyStaging.Mapping;");
                writer.WriteLine("using NpgsqlTypes;");
                writer.WriteLine();
                writer.WriteLine($"namespace {projectName}.Model");
                writer.WriteLine("{");
                writer.WriteLine($"\t[EntityMapping(name: \"{this.table.name}\", Schema = \"{this.schemaName}\")]");
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
                    string _type = item.RelType == "char" || item.RelType == "char?" ? "string" : item.RelType;
                    writer.WriteLine($"\t\tpublic {_type} {item.Field.ToUpperPascal()} {{ get; set; }}");
                    writer.WriteLine();
                }
                if (this.table.type == "table")
                {
                    string dalPath = $"{ projectName }.DAL.";
                    Hashtable ht = new Hashtable();
                    foreach (var item in consList)
                    {
                        string f_dalName = CreateName(item.nspname, item.table_name);
                        string pname = $"{item.table_name.ToUpperPascal()}";
                        string propertyName = f_dalName;
                        if (ht.ContainsKey(propertyName) || _classname == propertyName)
                        {
                            propertyName += "By" + item.conname.ToUpperPascal();
                        }


                        string tmp_var = propertyName.ToLowerPascal();
                        writer.WriteLine($"\t\tprivate {f_dalName}Model {tmp_var} = null;");
                        writer.WriteLine($"\t\t[ForeignKeyMapping(name: \"{item.conname}\"), JsonIgnore] public {f_dalName}Model {propertyName} {{ get {{ if ({tmp_var} == null) {tmp_var} = {dalPath}{f_dalName}.Context.Where(f => f.{item.ref_column.ToUpperPascal()} == this.{item.conname.ToUpperPascal()}).ToOne(); return {tmp_var}; }} }}");
                        writer.WriteLine();
                        ht.Add(propertyName, "");
                    }

                    List<string> d_key = new List<string>();
                    foreach (var item in pkList)
                    {
                        FieldInfo fs = fieldList.FirstOrDefault(f => f.Field == item.Field);
                        d_key.Add("this." + fs.Field.ToUpperPascal());
                    }
                    string dalName = CreateName();
                    string updateName = $"{dalPath}{dalName}.{dalName}UpdateBuilder";
                    writer.WriteLine($"\t\t[NonDbColumnMapping, JsonIgnore] public {updateName} UpdateBuilder {{ get {{ return new {updateName}({string.Join(",", d_key)}); }} }}");
                    writer.WriteLine();
                    writer.WriteLine($"\t\tpublic {_classname} Insert() {{ return {dalPath}{dalName}.Insert(this); }}");
                    writer.WriteLine();
                }
                writer.WriteLine("\t}");
                writer.WriteLine("}");
                writer.Flush();

                CreateDal();
            }
        }

        private string CreateName(string schema, string tableName, string separator = "")
        {
            string _classname = string.Empty;
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
        private string CreateName()
        {
            return CreateName(this.schemaName, this.table.name);
        }

        protected void CreateDal()
        {
            string _classname = CreateName();
            string _model_classname = _classname + "Model";
            string _fileName = $"{dalpath}/{_classname}.cs";
            using (StreamWriter writer = new StreamWriter(File.Create(_fileName), System.Text.Encoding.UTF8))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using Newtonsoft.Json;");
                writer.WriteLine("using Newtonsoft.Json.Linq;");
                writer.WriteLine("using MyStaging;");
                writer.WriteLine("using MyStaging.Helpers;");
                writer.WriteLine("using MyStaging.Common;");
                writer.WriteLine("using NpgsqlTypes;");
                writer.WriteLine("using System.Linq.Expressions;");
                writer.WriteLine($"using {projectName}.Model;");
                writer.WriteLine();
                writer.WriteLine($"namespace {projectName}.DAL");
                writer.WriteLine("{");
                writer.WriteLine($"\tpublic partial class {_classname} : QueryContext<{_model_classname}>");
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
                writer.WriteLine($"\t\tconst string insertCmdText = \"{insert_sql}\";");

                StringBuilder sb_primarykey = new StringBuilder();
                for (int i = 0; i < pkList.Count; i++)
                {
                    var item = pkList[i];
                    sb_primarykey.Append($"{item.Field}=@{item.Field}");
                    if (pkList.Count > i + 1)
                        sb_primarykey.Append(" and ");
                }
                string pkString = sb_primarykey.ToString();

                writer.WriteLine($"\t\tconst string deleteCmdText = \"DELETE FROM \\\"{this.schemaName}\\\".\\\"{this.table.name}\\\" WHERE {pkString}\";");
                writer.WriteLine($"\t\tpublic static {_classname} Context {{ get {{ return new {_classname}(); }} }}");
                writer.WriteLine();

                foreach (var item in fieldList)
                {
                    if (item.Is_array)
                    {
                        writer.WriteLine($"\t\tpublic {_classname} Where{item.Field.ToUpperPascal()}Any(params {item.RelType} {item.Field})");
                        writer.WriteLine("\t\t{");
                        writer.WriteLine($"\t\t\t if ({item.Field} == null || {item.Field}.Length == 0) return this;");
                        writer.WriteLine($"\t\t\t string text = JoinTo({item.Field}, NpgsqlDbType.{item.PgDbType}, \"{item.Db_type}\");");
                        writer.WriteLine($"\t\t\t base.Where($\"{item.Field} @> array[{{text}}]\");");
                        writer.WriteLine($"\t\t\t return this;");
                        writer.WriteLine("\t\t}");
                        writer.WriteLine();
                    }
                }

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
                string specificType = GetspecificType(item);
                string ap = item.Is_array ? " | NpgsqlDbType.Array" : "";
                writer.WriteLine($"\t\t\t{_cn}.AddParameter(\"{ item.Field}\", NpgsqlDbType.{item.PgDbType}{ap}, model.{item.Field.ToUpperPascal()}, {item.Length}, {specificType});");
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
            string updateName = CreateName() + "UpdateBuilder";
            if (d_key.Count > 0)
            {
                writer.WriteLine($"\t\tpublic static {updateName} Update({string.Join(",", d_key)})");
                writer.WriteLine("\t\t{");
                writer.WriteLine($"\t\t\treturn new {updateName}({string.Join(",", d_key_fields)});");
                writer.WriteLine("\t\t}");
                writer.WriteLine();
            }

            writer.WriteLine($"\t\tpublic static {updateName} UpdateBuilder {{ get {{ return new {updateName}(); }} }}");
            writer.WriteLine();


            writer.WriteLine($"\t\tpublic class {updateName} : UpdateBuilder<{class_model.ToUpperPascal()}>");
            writer.WriteLine("\t\t{");
            writer.WriteLine($"\t\t\tpublic {updateName}({string.Join(",", d_key)})");
            writer.WriteLine("\t\t\t{");
            if (pkList.Count > 0)
            {
                writer.Write($"\t\t\t\tbase.Where(f => ");
                for (int i = 0; i < pkList.Count; i++)
                {
                    var item = pkList[i];
                    writer.Write($"f.{item.Field.ToUpperPascal()} == {item.Field}");
                    if (i + 1 < pkList.Count)
                    {
                        writer.Write(" && ");
                    }
                }
                writer.Write(");\n");
            }
            writer.WriteLine("\t\t\t}");

            if (d_key.Count > 0)
            {
                writer.WriteLine();
                writer.WriteLine($"\t\t\tpublic {updateName}() {{ }}");
                writer.WriteLine();
            }

            writer.WriteLine($"\t\t\tpublic new {updateName} Where(Expression<Func<{class_model.ToUpperPascal()}, bool>> predicate)");
            writer.WriteLine("\t\t\t{");
            writer.WriteLine($"\t\t\t\t base.Where(predicate);");
            writer.WriteLine($"\t\t\t\t return this;");
            writer.WriteLine("\t\t\t}");
            writer.WriteLine();

            foreach (var item in fieldList)
            {
                if (item.Is_identity) continue;
                NpgsqlDbType _dbtype = PgsqlType.SwitchToSql(item.Data_Type, item.Db_type);

                writer.WriteLine($"\t\t\tpublic {updateName} Set{item.Field.ToUpperPascal()}({item.RelType} {item.Field})");
                writer.WriteLine("\t\t\t{");
                string specificType = GetspecificType(item);
                string ap = item.Is_array ? " | NpgsqlDbType.Array" : "";
                writer.WriteLine($"\t\t\t\treturn base.SetField(\"{ item.Field}\", NpgsqlDbType.{_dbtype}{ap}, {item.Field}, {item.Length}, {specificType}) as {updateName};");
                writer.WriteLine("\t\t\t}");

                if (item.Is_array)
                {
                    writer.WriteLine($"\t\t\tpublic {updateName} Set{item.Field.ToUpperPascal()}Append({item.CsType} {item.Field})");
                    writer.WriteLine("\t\t\t{");
                    writer.WriteLine($"\t\t\t\treturn base.SetArrayAppend(\"{ item.Field}\", NpgsqlDbType.{_dbtype}, {item.Field}, {item.Length}, {specificType}) as {updateName};");
                    writer.WriteLine("\t\t\t}");

                    writer.WriteLine($"\t\t\tpublic {updateName} Set{item.Field.ToUpperPascal()}Remove({item.CsType} {item.Field})");
                    writer.WriteLine("\t\t\t{");
                    writer.WriteLine($"\t\t\t\treturn base.SetArrayRemove(\"{ item.Field}\", NpgsqlDbType.{_dbtype}, {item.Field}, {item.Length}, {specificType}) as {updateName};");
                    writer.WriteLine("\t\t\t}");
                }

            }
            writer.WriteLine("\t\t}");
        }

        protected void Delete_Generator(StreamWriter writer, string class_model, string className)
        {
            if (pkList.Count > 0)
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
                    string specificType = GetspecificType(fi);
                    string ap = fi.Is_array ? " | NpgsqlDbType.Array" : "";
                    writer.WriteLine($"\t\t\t{_cn}.AddParameter(\"{ item.Field}\", NpgsqlDbType.{fi.PgDbType}{ap}, {item.Field}, {fi.Length}, {specificType});");
                }
                writer.WriteLine($"\t\t\treturn {_cn}.ExecuteNonQuery(deleteCmdText);");
                writer.WriteLine("\t\t}");
            }
            string deletebuilder = $"DeleteBuilder<{class_model}>";
            writer.WriteLine($"\t\tpublic static {deletebuilder} DeleteBuilder {{ get {{ return new {deletebuilder}(); }} }}");
        }

        #region primary key / constraint
        private void Get_Fields()
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
            _sqltext = string.Format(_sqltext, this.schemaName, this.table.name);


            PgSqlHelper.ExecuteDataReader(dr =>
            {
                FieldInfo fi = new FieldInfo();
                fi.Oid = Convert.ToInt32(dr["oid"]);
                fi.Field = dr["field"].ToString();
                fi.Length = Convert.ToInt32(dr["length"].ToString());
                fi.Is_not_null = Convert.ToBoolean(dr["notnull"]);
                fi.Comment = dr["comment"].ToString();
                fi.Data_Type = dr["data_type"].ToString();
                fi.Db_type = dr["type"].ToString();
                fi.Db_type = fi.Db_type.StartsWith("_") ? fi.Db_type.Remove(0, 1) : fi.Db_type;
                fi.PgDbType = PgsqlType.SwitchToSql(fi.Data_Type, fi.Db_type);
                fi.Is_identity = dr["is_identity"].ToString() == "YES";
                fi.Is_array = dr["typcategory"].ToString() == "A";
                fi.Is_enum = fi.Data_Type == "e";

                fi.CsType = PgsqlType.SwitchToCSharp(fi.Db_type);

                if (fi.Is_enum) fi.CsType = fi.CsType.ToUpperPascal();
                string _notnull = "";
                if (
                fi.CsType != "string"
                && fi.CsType != "JToken"
                && !fi.Is_array
                && fi.CsType != "System.Net.IPAddress"
                && fi.CsType != "System.Net.NetworkInformation.PhysicalAddress"
                && fi.CsType != "System.Xml.Linq.XDocument"
                && fi.CsType != "System.Collections.BitArray"
                && fi.CsType != "object"
                )
                    _notnull = fi.Is_not_null ? "" : "?";

                string _array = fi.Is_array ? "[]" : "";
                fi.RelType = $"{fi.CsType}{_notnull}{_array}";
                // dal
                this.fieldList.Add(fi);
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
WHERE b.nspname='{0}' and a.relname='{1}');"
, this.schemaName, this.table.name);


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
