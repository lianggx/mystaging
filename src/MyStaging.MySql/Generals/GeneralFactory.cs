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

namespace MyStaging.MySql.Generals
{
    public class GeneralFactory : IGeneralFactory
    {
        private DbContext dbContext;

        public void Initialize(ProjectConfig config)
        {
            Tables = new List<TableInfo>();
            StagingOptions options = new StagingOptions(config.ProjectName, config.ConnectionString)
            {
                Provider = ProviderType.MySql
            };

            string schema = new MySqlConnection(options.Master).Database;
            dbContext = new MySqlDbContext(options);

            #region dir

            CheckNotNull.NotEmpty(config.ProjectName, nameof(config.ProjectName));

            if (config.Mode == GeneralMode.Db)
            {
                CheckNotNull.NotEmpty(config.OutputDir, nameof(config.OutputDir));
                Config = new GeneralConfig
                {
                    OutputDir = config.OutputDir,
                    ProjectName = config.ProjectName,
                    ModelPath = config.OutputDir
                };

                if (!Directory.Exists(Config.ModelPath))
                    Directory.CreateDirectory(Config.ModelPath);
            }
            #endregion

            #region Tables

            string _sqltext = $@"SELECT TABLE_SCHEMA,TABLE_NAME,(CASE WHEN TABLE_TYPE = 'BASE TABLE' THEN 'TABLE'ELSE TABLE_TYPE END ) AS TABLE_TYPE 
FROM information_schema.`TABLES` WHERE TABLE_SCHEMA = '{schema}'";
            dbContext.Execute.ExecuteDataReader(dr =>
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

            var fileName = config.ProjectName + ".dll";
            var dir = System.IO.Directory.GetCurrentDirectory();

            var providerFile = System.IO.Directory.GetFiles(dir, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(providerFile))
                throw new FileNotFoundException($"在 {dir} 搜索不到文件 {fileName}");

            var types = Assembly.LoadFrom(providerFile).GetTypes();
            foreach (var t in types)
            {
                var tableAttribute = t.GetCustomAttribute<TableAttribute>();
                if (tableAttribute == null)
                    continue;

                var newTable = new TableInfo
                {
                    Name = tableAttribute.Name,
                    Schema = tableAttribute.Schema
                };

                SerializeField(newTable, t);

                var oldTable = Tables.Where(f => f.Schema == newTable.Schema && f.Name == newTable.Name).FirstOrDefault();
                if (oldTable == null) // CREATE
                    DumpTable(newTable, ref sb);
                else // ALTER
                    DumpAlter(newTable, oldTable, ref sb);
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
                dbContext.Execute.ExecuteNonQuery(CommandType.Text, sql);
            }
        }

        private void DumpAlter(TableInfo newTable, TableInfo oldTable, ref StringBuilder sb)
        {
            var alterSql = $"ALTER TABLE {newTable.Schema}.{newTable.Name}";

            // 常规
            foreach (var newFi in newTable.Fields)
            {
                var oldFi = oldTable.Fields.Where(f => f.Name == newFi.Name).FirstOrDefault();
                var notNull = newFi.NotNull ? "NOT NULL" : "NULL";
                var realType = newFi.DbTypeFull ?? newFi.DbType;
                if (oldFi == null)
                {
                    sb.AppendLine(alterSql + $" ADD {newFi.Name} {realType};");
                    sb.AppendLine(alterSql + $" MODIFY {newFi.Name} {realType} {notNull};");
                }
                else
                {
                    if (oldFi.DbTypeFull != newFi.DbTypeFull)
                        sb.AppendLine(alterSql + $" ALTER {newFi.Name} TYPE {realType};");
                    if (oldFi.NotNull != newFi.NotNull)
                    {
                        sb.AppendLine(alterSql + $" MODIFY {newFi.Name} {realType} {notNull};");
                    }
                }
            }

            // 检查旧约束
            List<string> primaryKeys = new List<string>();
            foreach (var c in oldTable.Constraints)
            {
                var constraint = newTable.Fields.Where(f => f.Name == c.Field).FirstOrDefault();
                if (constraint == null)
                {
                    sb.AppendLine(alterSql + $" DROP CONSTRAINT {c.Name};");
                }
            }

            // 检查新约束
            var pks = newTable.Fields.Where(f => f.PrimaryKey);
            foreach (var p in pks)
            {
                var constraint = oldTable.Constraints.Where(f => f.Field == p.Name).FirstOrDefault();
                if (constraint == null)
                {
                    sb.AppendLine(alterSql + $" ADD CONSTRAINT pk_{newTable.Name} PRIMARY KEY({p.Name});");
                }
            }
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
                    {
                        fi.NotNull = genericAttrs.Where(f => f == typeof(RequiredAttribute) || f == typeof(KeyAttribute)).FirstOrDefault() != null;
                    }
                    else
                    {
                        fi.NotNull = pi.PropertyType.IsValueType;
                    }
                }
                fi.PrimaryKey = genericAttrs.Where(f => f == typeof(KeyAttribute)).FirstOrDefault() != null;
                var columnAttribute = customAttributes.Where(f => f.GetType() == typeof(ColumnAttribute)).FirstOrDefault();
                if (columnAttribute != null)
                {
                    var colAttribute = ((ColumnAttribute)columnAttribute);
                    fi.DbType = fi.DbTypeFull = colAttribute.TypeName;
                }
                else
                {
                    fi.DbType = MysqlType.GetDbType(fi.CsType.Replace("[]", ""));
                    fi.DbTypeFull = GetFullDbType(fi);
                }
                fi.IsArray = fi.CsType.Contains("[]");

                table.Fields.Add(fi);
            }
        }

        private void DumpTable(TableInfo table, ref StringBuilder sb)
        {
            sb.AppendLine($"CREATE TABLE {table.Schema}.{table.Name}");
            sb.AppendLine("(");
            int length = table.Fields.Count;
            List<string> keys = new List<string>();
            for (int i = 0; i < length; i++)
            {
                var fi = table.Fields[i];

                sb.AppendFormat(" `{0}` {1} {2}{3} {4}",
                    fi.Name,
                    fi.DbType,
                    fi.IsArray ? "[]" : "",
                    fi.PrimaryKey || fi.NotNull ? "NOT NULL" : "DEFAULT NULL",
                    (i + 1 == length) ? "" : ","
                    );
                sb.AppendLine();
                if (fi.PrimaryKey)
                {
                    keys.Add(string.Format("`{0}`", fi.Name));
                }
            }

            if (keys.Count() > 0)
            {
                sb.AppendLine($" ,PRIMARY KEY ({string.Join(", ", keys)})");
            }
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
                {
                    fullType = $"{fi.DbType}({fi.Length},{fi.Numeric_scale})";
                }
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
                writer.WriteLine("using Newtonsoft.Json.Linq;");
                writer.WriteLine();
                writer.WriteLine($"namespace {Config.ProjectName}");
                writer.WriteLine("{");
                writer.WriteLine($"\tpublic class {contextName} : DbContext");
                writer.WriteLine("\t{");
                writer.WriteLine($"\t\tpublic {contextName}(StagingOptions options) : base(options, ProviderType.MySql)");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t}");
                writer.WriteLine();

                foreach (var table in Tables)
                {
                    writer.WriteLine($"\t\tpublic DbSet<{table.Name.ToUpperPascal()}> {table.Name.ToUpperPascal()} {{ get; set; }}");
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
                                             from information_schema.`COLUMNS` where TABLE_SCHEMA='{table.Schema}' and TABLE_NAME='{table.Name}';";

            _sqltext = string.Format(_sqltext, table.Schema, table.Name);
            dbContext.Execute.ExecuteDataReader(dr =>
            {
                DbFieldInfo fi = new DbFieldInfo
                {
                    Oid = Convert.ToInt32(dr["ORDINAL_POSITION"]),
                    Name = dr["COLUMN_NAME"].ToString(),
                    Length = Convert.ToInt64(dr["LENGTEH"].ToString()),
                    NotNull = dr["IS_NULLABLE"].ToString() == "NO",
                    Comment = dr["COLUMN_COMMENT"].ToString(),
                    Numeric_scale = Convert.ToInt32(dr["NUMERIC_SCALE"].ToString()),
                    DbType = dr["DATA_TYPE"].ToString()
                };

                fi.CsType = MysqlType.SwitchToCSharp(fi.DbType);
                if (!fi.NotNull && fi.CsType != "string" && fi.CsType != "byte[]" && fi.CsType != "JToken")
                {
                    fi.RelType = $"{fi.CsType}?";
                }
                else
                {
                    fi.RelType = fi.CsType;
                }

                if ((fi.RelType == "string" && (fi.Length != 0 && fi.Length != 255))
                     || (fi.Numeric_scale > 0)
                     || (MysqlType.ContrastType(fi.DbType) == null)
                     )
                {
                    fi.DbTypeFull = dr["COLUMN_TYPE"].ToString();
                }

                table.Fields.Add(fi);
            }, CommandType.Text, _sqltext);

            if (table.Type == TableType.Table)
                GetPrimarykey(table);
        }

        private void GetPrimarykey(TableInfo table)
        {
            string _sqltext = $@"SELECT COLUMN_NAME,CONSTRAINT_NAME FROM information_schema.KEY_COLUMN_USAGE where TABLE_SCHEMA = '{table.Schema}' and TABLE_NAME = '{table.Name}' AND CONSTRAINT_NAME = 'PRIMARY'";

            dbContext.Execute.ExecuteDataReader(dr =>
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
        public List<string> Filters { get; set; } = new List<string>() {
               "geometry_columns",
               "raster_columns",
               "spatial_ref_sys",
               "raster_overviews",
               "us_gaz",
               "topology",
               "zip_lookup_all",
               "pg_toast",
               "pg_temp_1",
               "pg_toast_temp_1",
               "pg_catalog",
               "information_schema",
               "tiger",
               "tiger_data"
        };
        public GeneralConfig Config { get; set; }
        public List<TableInfo> Tables { get; set; }
        #endregion
    }
}
