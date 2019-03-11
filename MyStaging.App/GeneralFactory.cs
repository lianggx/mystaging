using MyStaging.App.DAL;
using MyStaging.App.Models;
using MyStaging.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MyStaging.App
{
    public class GeneralFactory
    {
        private static string schemaPath = string.Empty;
        private static string modelPath = string.Empty;
        private static string dalPath = string.Empty;
        private static string projectName = string.Empty;
        private static string outputDir = string.Empty;

        public static void Build(string outputdir, string projName)
        {
            if (string.IsNullOrEmpty(outputdir) || string.IsNullOrEmpty(projName))
                throw new ArgumentNullException("outputdir 和 projName", "不能为空");
            outputDir = outputdir;
            projectName = projName;

            CreateDir();
            CreateCsproj();
            EnumsDal.Generate(Path.Combine(outputdir, projName + ".db"), modelPath, GeneralFactory.projectName);

            List<string> schemaList = SchemaDal.Get_List();
            foreach (var schemaName in schemaList)
            {
                Console.WriteLine("正在生成模式：{0}", schemaName);
                List<TableViewModel> tableList = GetTables(schemaName);
                foreach (var item in tableList)
                {
                    Console.WriteLine("{0}:{1}", item.Type, item.Name);
                    TablesDal td = new TablesDal(GeneralFactory.projectName, modelPath, schemaPath, dalPath, schemaName, item);
                    td.Create();
                }
            }
        }

        private static void CreateDir()
        {
            modelPath = Path.Combine(outputDir, projectName + ".db", "Model", "Build");
            schemaPath = Path.Combine(outputDir, projectName + ".db", "Model", "Schemas");
            dalPath = Path.Combine(outputDir, projectName + ".db", "DAL", "Build");
            string[] ps = { modelPath, schemaPath, dalPath };
            for (int i = 0; i < ps.Length; i++)
            {
                if (!Directory.Exists(ps[i]))
                    Directory.CreateDirectory(ps[i]);
            }
        }

        private static void CreateCsproj()
        {
            string path = Path.Combine(outputDir, $"{projectName}.db");

            string csproj = Path.Combine(path, $"{projectName}.db.csproj");
            if (File.Exists(csproj))
                return;

            using (StreamWriter writer = new StreamWriter(File.Create(csproj), System.Text.Encoding.UTF8))
            {
                writer.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
                writer.WriteLine("\t<PropertyGroup>");
                writer.WriteLine($"\t\t<TargetFramework>netstandard2.0</TargetFramework>");
                writer.WriteLine($"\t\t<Authors></Authors>");
                writer.WriteLine($"\t\t <Company></Company>");
                writer.WriteLine("\t</PropertyGroup>");
                writer.WriteLine();
                writer.WriteLine("\t<ItemGroup>");
                writer.WriteLine("\t\t<PackageReference Include=\"MyStaging\" Version=\"1.0.0\" />");
                writer.WriteLine("\t</ItemGroup>");
                writer.WriteLine("</Project>");
            }
        }

        private static List<TableViewModel> GetTables(string schema)
        {
            string _sqltext = $@"SELECT table_name,'table' as type FROM INFORMATION_SCHEMA.tables WHERE table_schema='{schema}' AND table_type='BASE TABLE'
UNION ALL
SELECT table_name,'view' as type FROM INFORMATION_SCHEMA.views WHERE table_schema = '{schema}'";
            List<TableViewModel> tableList = new List<TableViewModel>();
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                TableViewModel model = new TableViewModel() { Name = dr["table_name"].ToString(), Type = dr["type"].ToString() };
                tableList.Add(model);
            }, CommandType.Text, _sqltext);

            return tableList;
        }

    }
}
