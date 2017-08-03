using MyStaging;
using MyStaging.App.Models;
using MyStaging.Common;
using MyStaging.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyStaging.App.DAL
{
    public class EnumsDal
    {
        private static string projectName = string.Empty;
        private static string modelPath = string.Empty;
        private static string rootPath = string.Empty;

        public static void Generate(string rootpath, string modelpath, string projName)
        {
            rootPath = rootpath;
            modelPath = modelpath;
            projectName = projName;

            string _sqltext = @"
select a.oid,a.typname,b.nspname from pg_type a 
INNER JOIN pg_namespace b on a.typnamespace = b.oid 
where a.typtype = 'e' order by oid asc";

            List<EnumTypeInfo> list = new List<EnumTypeInfo>();
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                list.Add(new EnumTypeInfo()
                {
                    oid = Convert.ToInt32(dr["oid"]),
                    typename = dr["typname"].ToString(),
                    nspname = dr["nspname"].ToString()
                });
            }, System.Data.CommandType.Text, _sqltext);

            string _fileName = Path.Combine(modelpath, "_Enums.cs");
            using (StreamWriter writer = new StreamWriter(File.Create(_fileName)))
            {
                writer.WriteLine("using System;");
                writer.WriteLine();
                writer.WriteLine($"namespace {projectName}.Model");
                writer.WriteLine("{");

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    writer.WriteLine($"\tpublic enum {item.typename.ToUpperPascal()}");
                    writer.WriteLine("\t{");
                    string sql = $"select oid,enumlabel from pg_enum WHERE enumtypid = {item.oid} ORDER BY oid asc";
                    PgSqlHelper.ExecuteDataReader(dr =>
                    {
                        string c = i < list.Count ? "," : "";
                        writer.WriteLine($"\t\t{dr["enumlabel"]}{c}");
                    }, System.Data.CommandType.Text, sql);
                    writer.WriteLine("\t}");
                }
                writer.WriteLine("}");
            }

            GenerateMapping(list);
        }

        public static void GenerateMapping(List<EnumTypeInfo> list)
        {
            string _fileName = Path.Combine(rootPath, "_startup.cs");
            using (StreamWriter writer = new StreamWriter(File.Create(_fileName)))
            {
                writer.WriteLine($"using {projectName}.Model;");
                writer.WriteLine("using System;");
                writer.WriteLine("using Microsoft.Extensions.Logging;");
                writer.WriteLine("using MyStaging.Helpers;");
                writer.WriteLine("using Npgsql;");
                writer.WriteLine();
                writer.WriteLine($"namespace {projectName}");
                writer.WriteLine("{");
                writer.WriteLine("\tpublic class _startup");
                writer.WriteLine("\t{");
                writer.WriteLine("\t\tpublic static void Init(NLog.ILogger logger, string connectionString)");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tPgSqlHelper.InitConnection(logger, connectionString);");
                writer.WriteLine();
                writer.WriteLine("\t\t\tNpgsqlNameTranslator translator = new NpgsqlNameTranslator();");
                foreach (var item in list)
                {
                    writer.WriteLine($"\t\t\tNpgsqlConnection.MapEnumGlobally<{item.typename.ToUpperPascal()}>(\"{item.nspname}.{item.typename}\", translator);");
                }
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t}");
                writer.WriteLine("\tpublic partial class NpgsqlNameTranslator : INpgsqlNameTranslator");
                writer.WriteLine("\t{");
                writer.WriteLine("\t\tprivate string clrName;");
                writer.WriteLine("\t\tpublic string TranslateMemberName(string clrName)");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tthis.clrName = clrName;");
                writer.WriteLine("\t\t\treturn this.clrName;");
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t\tprivate string clrTypeName;");
                writer.WriteLine("\t\tpublic string TranslateTypeName(string clrName)");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tthis.clrTypeName = clrName;");
                writer.WriteLine("\t\t\treturn this.clrTypeName;");
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t}");
                writer.WriteLine("}"); // namespace end
            }
        }
    }
}
