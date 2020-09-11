using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Metadata;
using MyStaging.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MySql.Data.MySqlClient;
using MyStaging.DataAnnotations;

namespace MyStaging.MySql.Generals
{
    public class GeneralFactory : IGeneralFactory
    {
        private ProjectConfig config;

        public void Initialize(ProjectConfig config)
        {
            this.config = config;
            Tables = new List<TableInfo>();

            string schema = new MySqlConnection(config.ConnectionString).Database;
            #region dir

            CheckNotNull.NotEmpty(config.ContextName, nameof(config.ContextName));

            if (config.Mode == GeneralInfo.Db)
            {
                CheckNotNull.NotEmpty(config.OutputDir, nameof(config.OutputDir));
                Config = new GeneralConfig
                {
                    OutputDir = config.OutputDir,
                    ProjectName = config.ContextName,
                    ModelPath = config.OutputDir
                };

                if (!Directory.Exists(Config.ModelPath))
                    Directory.CreateDirectory(Config.ModelPath);
            }
            #endregion

            #region Tables

            string _sqltext = $@"SELECT TABLE_SCHEMA,TABLE_NAME,(CASE WHEN TABLE_TYPE = 'BASE TABLE' THEN 'TABLE'ELSE TABLE_TYPE END ) AS TABLE_TYPE 
FROM information_schema.`TABLES` WHERE TABLE_SCHEMA = '{schema}'";
            SQLContext.ExecuteDataReader(dr =>
              {
                  var table = new TableInfo()
                  {
                      Schema = dr["TABLE_SCHEMA"].ToString(),
                      Name = dr["TABLE_NAME"].ToString(),
                      Type = Enum.Parse<TableType>(dr["TABLE_TYPE"].ToString(), true)
                  };
                  GetFields(table);
                  Tables.Add(table);
              }, CommandType.Text, _sqltext);

            #endregion
        }

        public void DbFirst(ProjectConfig config)
        {
            Initialize(config);
            GenerateMapping();
            // Generral Entity
            foreach (var table in Tables)
            {
                Console.WriteLine("[{0}]{1}.{2}", table.Type, table.Schema, table.Name);
                EntityGeneral td = new EntityGeneral(Config, table);
                td.Create();
            }
        }

        public void CodeFirst(ProjectConfig config)
        {
            Initialize(config);

            StringBuilder sb = new StringBuilder();
            List<TableInfo> tables = new List<TableInfo>();

            var fileName = config.ContextName + ".dll";
            var dir = System.IO.Directory.GetCurrentDirectory();

            var providerFile = System.IO.Directory.GetFiles(dir, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(providerFile))
                throw new FileNotFoundException($"在 {dir} 搜索不到文件 {fileName}");

            var types = Assembly.LoadFrom(providerFile).GetTypes();
            List<TableInfo> entitys = new List<TableInfo>();
            foreach (var t in types)
            {
                var tableAttribute = t.GetCustomAttribute<TableAttribute>();
                if (tableAttribute == null)
                    continue;

                entitys.Add(new TableInfo
                {
                    Name = tableAttribute.Name,
                    Schema = tableAttribute.Schema,
                    EntityType = t
                });
            }

            foreach (var ent in entitys)
            {
                SerializeField(ent, ent.EntityType);

                var table = Tables.Where(f => f.Schema == ent.Schema && f.Name == ent.Name).FirstOrDefault();
                if (table == null) // CREATE
                    DumpTable(ent, ref sb);
                else // ALTER
                    DumpAlter(ent, table, ref sb);
            }

            // 删除实体
            foreach (var table in Tables)
            {
                if (entitys.Where(f => f.Schema == table.Schema && f.Name == table.Name).FirstOrDefault() == null)
                {
                    sb.AppendLine($"DROP TABLE {MyStagingUtils.GetTableName(table, ProviderType.MySql)};");
                }
            }

            var sql = sb.ToString();

            if (string.IsNullOrEmpty(sql))
            {
                Console.WriteLine("数据模型没有可执行的更改.");
            }
            else
            {
                Console.WriteLine("------------------SQL------------------");
                Console.WriteLine(sql);
                Console.WriteLine("------------------SQL END------------------");

                SQLContext.ExecuteNonQuery(CommandType.Text, sql);
            }
        }

        private void DumpAlter(TableInfo newTable, TableInfo oldTable, ref StringBuilder sb)
        {

            var alterSql = $"ALTER TABLE {MyStagingUtils.GetTableName(newTable, ProviderType.MySql)}";

            // 常规
            foreach (var newFi in newTable.Fields)
            {
                var oldFi = oldTable.Fields.Where(f => f.Name == newFi.Name).FirstOrDefault();
                var notNull = newFi.NotNull ? " NOT NULL" : "";
                var realType = MysqlType.GetRealType(newFi);
                if (oldFi == null)
                    sb.AppendLine($"{alterSql} ADD COLUMN `{newFi.Name}` {realType} {notNull};");
                else if (oldFi.DbType != newFi.DbType || oldFi.NotNull != newFi.NotNull)
                    sb.AppendLine($"{alterSql} MODIFY COLUMN `{newFi.Name}` {realType}{notNull};");
            }

            // 移除旧字段
            foreach (var oldFi in oldTable.Fields)
            {
                if (newTable.Fields.Where(f => f.Name == oldFi.Name).FirstOrDefault() == null)
                    sb.AppendLine($"{alterSql} DROP COLUMN `{oldFi.Name}`;");
            }

            // PRIMARY KEY
            var changed = PKChanged(oldTable, newTable);
            if (changed)
            {
                var newPk = newTable.Fields.Where(f => f.PrimaryKey).ToList();

                if (newPk.Count > 0)
                {
                    // 删除数据库约束
                    if (oldTable.Fields.Where(f => f.PrimaryKey).FirstOrDefault() != null)
                    {
                        var auto_increment = oldTable.Fields.Where(f => f.PrimaryKey && f.AutoIncrement).FirstOrDefault();
                        if (auto_increment != null)
                        {
                            sb.AppendLine($"{alterSql} MODIFY COLUMN `{auto_increment.Name}` {auto_increment.DbType};");
                        }
                        sb.AppendLine($"{alterSql} DROP PRIMARY KEY;");
                    }

                    // 增加实体约束
                    if (newPk.Count == 1)
                    {
                        var auto_increment = newPk[0].AutoIncrement ? " AUTO_INCREMENT" : "";
                        sb.AppendLine($"{alterSql} MODIFY {newPk[0].Name} {newPk[0].DbType} PRIMARY KEY{auto_increment};");
                    }
                    else if (newPk.Count > 1)
                    {
                        var pks = string.Join(",", newPk.Select(f => "`" + f.Name + "`"));
                        sb.AppendLine($"{alterSql} Add PRIMARY KEY({pks});");
                    }
                }
            }
        }

        private bool PKChanged(TableInfo oldTable, TableInfo newTable)
        {
            bool changed = false;

            // 检查数据库结构
            foreach (var fi in oldTable.Fields)
            {
                if (!fi.PrimaryKey)
                    continue;

                // PK
                if (newTable.Fields.Where(f => f.Name == fi.Name && f.PrimaryKey && f.AutoIncrement == fi.AutoIncrement).FirstOrDefault() == null)
                {
                    changed = true;
                    break;
                }
            }

            if (!changed)
            {
                // 检查实体结构
                foreach (var fi in newTable.Fields)
                {
                    if (!fi.PrimaryKey)
                        continue;

                    // PK
                    if (oldTable.Fields.Where(f => f.Name == fi.Name && f.PrimaryKey && f.AutoIncrement == fi.AutoIncrement).FirstOrDefault() == null)
                    {
                        changed = true;
                        break;
                    }
                }
            }

            return changed;
        }

        private void SerializeField(TableInfo table, Type type)
        {
            var properties = MyStagingUtils.GetDbFields(type);
            foreach (var pi in properties)
            {
                var fi = new DbFieldInfo();
                fi.Name = pi.Name;
                var customAttributes = pi.GetCustomAttributes();
                var genericAttrs = customAttributes.Select(f => f.GetType()).ToArray();
                if (pi.PropertyType.Name == "Nullable`1")
                {
                    fi.NotNull = false;
                    fi.CsType = pi.PropertyType.GenericTypeArguments[0].Name;
                }
                else
                {
                    fi.CsType = pi.PropertyType.Name;
                    if (pi.PropertyType == typeof(string))
                        fi.NotNull = genericAttrs.Where(f => f == typeof(RequiredAttribute) || f == typeof(PrimaryKeyAttribute)).FirstOrDefault() != null;
                    else
                        fi.NotNull = pi.PropertyType.IsValueType;
                }

                fi.PrimaryKey = genericAttrs.Where(f => f == typeof(PrimaryKeyAttribute)).FirstOrDefault() != null;
                if (fi.PrimaryKey)
                {
                    var pk = pi.GetCustomAttribute<PrimaryKeyAttribute>();
                    fi.AutoIncrement = pk.AutoIncrement;
                }

                var columnAttribute = customAttributes.Where(f => f.GetType() == typeof(ColumnAttribute)).FirstOrDefault();

                if (columnAttribute != null)
                {
                    var colAttribute = ((ColumnAttribute)columnAttribute);
                    fi.DbType = fi.DbTypeFull = colAttribute.TypeName;
                    if (colAttribute.TypeName != "char(36)" && colAttribute.TypeName != "tinyint(1)")
                    {
                        var zero = colAttribute.TypeName.IndexOf("(");
                        if (zero > 0)
                            fi.DbType = colAttribute.TypeName.Substring(0, zero);
                    }
                }
                else
                {
                    fi.DbTypeFull = GetFullDbType(fi);
                    fi.DbType = MysqlType.GetDbType(fi.CsType);
                    if (fi.DbType == "varchar" || fi.DbType == "char")
                    {
                        fi.DbTypeFull = $"{fi.DbType}(255)";
                    }
                }

                table.Fields.Add(fi);
            }
        }

        private void DumpTable(TableInfo table, ref StringBuilder sb)
        {
            sb.AppendLine($"CREATE TABLE {MyStagingUtils.GetTableName(table, ProviderType.MySql)}");
            sb.AppendLine("(");
            int length = table.Fields.Count;
            List<string> keys = new List<string>();
            for (int i = 0; i < length; i++)
            {
                var fi = table.Fields[i];

                sb.AppendFormat(" `{0}` {1} {2}{3},\n",
                    fi.Name,
                    fi.DbTypeFull ?? fi.DbType,
                    fi.AutoIncrement ? "AUTO_INCREMENT" : "",
                    fi.PrimaryKey || fi.NotNull ? " NOT NULL" : ""
                    );

                if (fi.PrimaryKey)
                    keys.Add(string.Format("`{0}`", fi.Name));
            }

            if (keys.Count() > 0)
                sb.AppendLine($" PRIMARY KEY ({string.Join(", ", keys)})");
            else
                sb.Remove(sb.Length - 1, 1);

            sb.AppendLine(");");
        }

        private string GetFullDbType(DbFieldInfo fi)
        {
            string fullType = null;
            if (fi.Length > 0)
            {
                if (fi.Length != 255 && fi.CsType == "String")
                    fullType = $"{fi.DbType}({fi.Length})";
                else if (fi.CsType != "String" && fi.Numeric_scale > 0)
                    fullType = $"{fi.DbType}({fi.Length},{fi.Numeric_scale})";
            }

            return fullType;
        }

        public void GenerateMapping()
        {
            var contextName = $"{ Config.ProjectName }DbContext";
            string _startup_file = Path.Combine(Config.OutputDir, $"{contextName}.cs");
            using (StreamWriter writer = new StreamWriter(File.Create(_startup_file), System.Text.Encoding.UTF8))
            {
                writer.WriteLine($"using {Config.ProjectName}.Model;");
                writer.WriteLine("using System;");
                writer.WriteLine("using MyStaging.Core;");
                writer.WriteLine("using MyStaging.Common;");
                writer.WriteLine("using MyStaging.Metadata;");
                writer.WriteLine("using Newtonsoft.Json.Linq;");
                writer.WriteLine();
                writer.WriteLine($"namespace {Config.ProjectName}");
                writer.WriteLine("{");
                writer.WriteLine($"\tpublic partial class {contextName} : DbContext");
                writer.WriteLine("\t{");
                writer.WriteLine($"\t\tpublic {contextName}(StagingOptions options) : base(options, ProviderType.MySql)");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t}");
                writer.WriteLine();

                foreach (var table in Tables)
                {
                    var tableName = MyStagingUtils.ToUpperPascal(table.Name);
                    writer.WriteLine($"\t\tpublic DbSet<{tableName}> {tableName} {{ get; set; }}");
                }

                writer.WriteLine("\t}"); // class end
                writer.WriteLine("}"); // namespace end
            }
        }

        private void GetFields(TableInfo table)
        {
            string _sqltext = $@"SELECT 
                                            ORDINAL_POSITION
                                            ,COLUMN_NAME
                                            ,IS_NULLABLE
                                            ,COLUMN_TYPE
                                            ,(CASE WHEN COLUMN_TYPE IN ('tinyint(1)','char(36)') THEN COLUMN_TYPE ELSE DATA_TYPE END) AS DATA_TYPE
                                            ,COLUMN_COMMENT
                                            ,COALESCE(
	                                            CASE 
		                                            WHEN DATA_TYPE IN ('tinyint','smallint','mediumint','int','bigint','bit','double','float','decimal') THEN NUMERIC_PRECISION
		                                            WHEN DATA_TYPE IN ('date','time','year','timestamp','datetime') THEN DATETIME_PRECISION
		                                            ELSE CHARACTER_MAXIMUM_LENGTH
	                                            END
                                            ,0) AS LENGTEH
                                            ,COALESCE(NUMERIC_SCALE,0) AS NUMERIC_SCALE
                                            ,(EXTRA='auto_increment') as auto_increment
                                             from information_schema.`COLUMNS` where TABLE_SCHEMA='{table.Schema}' and TABLE_NAME='{table.Name}';";

            _sqltext = string.Format(_sqltext, table.Schema, table.Name);
            SQLContext.ExecuteDataReader(dr =>
            {
                DbFieldInfo fi = new DbFieldInfo
                {
                    Oid = Convert.ToInt32(dr["ORDINAL_POSITION"]),
                    Name = dr["COLUMN_NAME"].ToString(),
                    Length = Convert.ToInt64(dr["LENGTEH"].ToString()),
                    NotNull = dr["IS_NULLABLE"].ToString() == "NO",
                    Comment = dr["COLUMN_COMMENT"].ToString(),
                    Numeric_scale = Convert.ToInt32(dr["NUMERIC_SCALE"].ToString()),
                    DbType = dr["DATA_TYPE"].ToString(),
                    AutoIncrement = Convert.ToBoolean(dr["auto_increment"])
                };

                fi.CsType = MysqlType.SwitchToCSharp(fi.DbType);
                if (!fi.NotNull && fi.CsType != "string" && fi.CsType != "byte[]" && fi.CsType != "JToken")
                    fi.RelType = $"{fi.CsType}?";
                else
                    fi.RelType = fi.CsType;

                if ((fi.RelType == "string" && fi.Length != 0 && fi.Length != 255) || (fi.Numeric_scale > 0) || (MysqlType.ContrastType(fi.DbType) == null))
                    fi.DbTypeFull = dr["COLUMN_TYPE"].ToString();

                table.Fields.Add(fi);
            }, CommandType.Text, _sqltext);

            if (table.Type == TableType.Table)
                GetPrimarykey(table);
        }

        private void GetPrimarykey(TableInfo table)
        {
            string _sqltext = $@"SELECT COLUMN_NAME,CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE where TABLE_SCHEMA = '{table.Schema}' and TABLE_NAME = '{table.Name}' AND CONSTRAINT_NAME = 'PRIMARY'";

            SQLContext.ExecuteDataReader(dr =>
            {
                var constaint = new ConstraintInfo
                {
                    Field = dr["COLUMN_NAME"].ToString(),
                    Name = dr["CONSTRAINT_NAME"].ToString(),
                    Type = ConstraintType.PK
                };

                table.Constraints.Add(constaint);
                table.Fields.Where(f => f.Name == constaint.Field).First().PrimaryKey = true;

            }, CommandType.Text, _sqltext);
        }

        #region Properties
        public GeneralConfig Config { get; set; }
        public List<TableInfo> Tables { get; set; }
        private SQLExecute SQLContext => new SQLExecute(new MySqlConnection(config.ConnectionString));

        #endregion
    }
}
