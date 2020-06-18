using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Gen.Tool;
using MyStaging.Gen.Tool.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MyStaging.PostgreSQL.Generals
{
    public class GeneralFactory : IGeneralFactory
    {
        private DbContext dbContext;

        public void Initialize(ProjectConfig config)
        {
            StagingOptions options = new StagingOptions(config.ProjectName, config.ConnectionString)
            {
                Provider = config.Provider
            };
            dbContext = new PgDbContext(options);

            #region dir
            CheckNotNull.NotEmpty(config.OutputDir, nameof(config.OutputDir));
            CheckNotNull.NotEmpty(config.ProjectName, nameof(config.ProjectName));


            Config = new GeneralConfig
            {
                OutputDir = config.OutputDir,
                ProjectName = config.ProjectName,
                ModelPath = Path.Combine(config.OutputDir, "Models")
            };

            if (!Directory.Exists(Config.ModelPath))
                Directory.CreateDirectory(Config.ModelPath);
            #endregion

            #region Schemas
            string[] filters = new string[this.Filters.Count];
            for (int i = 0; i < Filters.Count; i++)
            {
                filters[i] = $"'{Filters[i]}'";
            }

            string sql = $@"SELECT schema_name FROM information_schema.schemata WHERE SCHEMA_NAME NOT IN({string.Join(",", filters)}) ORDER BY SCHEMA_NAME; ";

            List<string> schemas = new List<string>();
            dbContext.Execute.ExecuteDataReader(dr =>
            {
                schemas.Add(dr[0].ToString());
            }, CommandType.Text, sql);
            #endregion

            #region Tables
            foreach (var schema in schemas)
            {
                string _sqltext = $@"SELECT table_name,'table' as type FROM INFORMATION_SCHEMA.tables WHERE table_schema='{schema}' AND table_type='BASE TABLE'
UNION ALL
SELECT table_name,'view' as type FROM INFORMATION_SCHEMA.views WHERE table_schema = '{schema}'";
                dbContext.Execute.ExecuteDataReader(dr =>
                {
                    Tables.Add(new TableInfo()
                    {
                        Schema = schema,
                        Name = dr["table_name"].ToString(),
                        Type = dr["type"].ToString()
                    });
                }, CommandType.Text, _sqltext);

            }
            #endregion
        }

        public void Build(ProjectConfig config)
        {
            Initialize(config);
            GenerateMapping();

            // Generral Model
            foreach (var table in Tables)
            {
                Console.WriteLine("[{0}]{1}.{2}", table.Type, table.Schema, table.Name);
                EntityGeneral td = new EntityGeneral(dbContext, Config, table);
                td.Create();
            }
        }

        public void GenerateMapping()
        {
            string _sqltext = @"
select a.oid,a.typname,b.nspname from pg_type a 
INNER JOIN pg_namespace b on a.typnamespace = b.oid 
where a.typtype = 'e' order by oid asc";

            List<EnumTypeInfo> enums = new List<EnumTypeInfo>();
            dbContext.Execute.ExecuteDataReader(dr =>
            {
                enums.Add(new EnumTypeInfo()
                {
                    Oid = Convert.ToInt32(dr["oid"]),
                    TypeName = dr["typname"].ToString(),
                    NspName = dr["nspname"].ToString()
                });
            }, System.Data.CommandType.Text, _sqltext);

            if (enums.Count > 0)
            {
                string _fileName = Path.Combine(Config.ModelPath, "_Enums.cs");
                using StreamWriter writer = new StreamWriter(File.Create(_fileName), System.Text.Encoding.UTF8);
                writer.WriteLine("using System;");
                writer.WriteLine();
                writer.WriteLine($"namespace {Config.ProjectName}.Model");
                writer.WriteLine("{");

                for (int i = 0; i < enums.Count; i++)
                {
                    var item = enums[i];
                    writer.WriteLine($"\tpublic enum {item.TypeName.ToUpperPascal()}");
                    writer.WriteLine("\t{");
                    string sql = $"select oid,enumlabel from pg_enum WHERE enumtypid = {item.Oid} ORDER BY oid asc";
                    dbContext.Execute.ExecuteDataReader(dr =>
                    {
                        string c = i < enums.Count ? "," : "";
                        writer.WriteLine($"\t\t{dr["enumlabel"]}{c}");
                    }, CommandType.Text, sql);
                    writer.WriteLine("\t}");
                }
                writer.WriteLine("}");
            }

            var contextName = $"{ Config.ProjectName }DbContext";
            string _startup_file = Path.Combine(Config.OutputDir, $"{contextName}.cs");
            using (StreamWriter writer = new StreamWriter(File.Create(_startup_file), System.Text.Encoding.UTF8))
            {
                writer.WriteLine($"using {Config.ProjectName}.Model;");
                writer.WriteLine("using System;");
                writer.WriteLine("using Npgsql;");
                writer.WriteLine("using MyStaging.Core;");
                writer.WriteLine("using MyStaging.Common;");
                writer.WriteLine("using Newtonsoft.Json.Linq;");
                writer.WriteLine();
                writer.WriteLine($"namespace {Config.ProjectName}");
                writer.WriteLine("{");
                writer.WriteLine($"\tpublic class {contextName} : DbContext");
                writer.WriteLine("\t{");
                writer.WriteLine($"\t\tpublic {contextName}(StagingOptions options) : base(options)");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tInitializerMapping();");
                writer.WriteLine("\t\t}");
                writer.WriteLine();
                writer.WriteLine($"\t\tpublic void InitializerMapping()");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tType[] jsonTypes = { typeof(JToken), typeof(JObject), typeof(JArray) };");
                writer.WriteLine("\t\t\tNpgsqlNameTranslator translator = new NpgsqlNameTranslator();");
                writer.WriteLine("\t\t\tNpgsqlConnection.GlobalTypeMapper.UseJsonNet(jsonTypes);");

                foreach (var table in Tables)
                {
                    if (table.Name == "geometry_columns")
                    {
                        writer.WriteLine($"\t\t\tNpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();");
                        break;
                    }
                }

                if (enums.Count > 0)
                {
                    writer.WriteLine();
                    foreach (var item in enums)
                    {
                        writer.WriteLine($"\t\t\tNpgsqlConnection.GlobalTypeMapper.MapEnum<{item.TypeName.ToUpperPascal()}>(\"{item.NspName}.{item.TypeName}\", translator);");
                    }
                }

                writer.WriteLine("\t\t}"); // InitializerMapping end
                writer.WriteLine();

                foreach (var table in Tables)
                {
                    writer.WriteLine($"\t\tpublic DbSet<{table.Name.ToUpperPascal()}Model> {table.Name.ToUpperPascal()} {{ get; set; }}");
                }

                writer.WriteLine("\t}"); // class end
                writer.WriteLine("\tpublic partial class NpgsqlNameTranslator : INpgsqlNameTranslator");
                writer.WriteLine("\t{");
                writer.WriteLine("\t\tpublic string TranslateMemberName(string clrName) => clrName;");
                writer.WriteLine("\t\tpublic string TranslateTypeName(string clrTypeName) => clrTypeName;");
                writer.WriteLine("\t}");
                writer.WriteLine("}"); // namespace end
            }
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
        public List<TableInfo> Tables { get; set; } = new List<TableInfo>();
        #endregion
    }
}
