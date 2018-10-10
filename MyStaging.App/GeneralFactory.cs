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
            EnumsDal.Generate(Path.Combine(outputdir, projName, projName + ".db"), modelPath, GeneralFactory.projectName);

            List<string> schemaList = SchemaDal.Get_List();
            foreach (var schemaName in schemaList)
            {
                Console.WriteLine("正在生成模式：{0}", schemaName);
                List<TableViewModel> tableList = GetTables(schemaName);
                foreach (var item in tableList)
                {
                    TablesDal td = new TablesDal(GeneralFactory.projectName, modelPath, dalPath, schemaName, item);
                    td.Generate();
                }
            }
        }

        private static void CreateDir()
        {
            modelPath = Path.Combine(outputDir, projectName, projectName + ".db", "Model", "Build");
            dalPath = Path.Combine(outputDir, projectName, projectName + ".db", "DAL", "Build");
            string[] ps = { modelPath, dalPath };
            for (int i = 0; i < ps.Length; i++)
            {
                if (!Directory.Exists(ps[i]))
                    Directory.CreateDirectory(ps[i]);
            }
        }

        private static void CreateCsproj()
        {
            string path = Path.Combine(outputDir, projectName, $"{projectName}.db");

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
                writer.WriteLine("\t\t<ProjectReference Include=\"..\\MyStaging\\MyStaging.csproj\" />");
                writer.WriteLine("\t</ItemGroup>");
                writer.WriteLine("</Project>");
            }

            // unzip
            string mystaging_file = Path.Combine(outputDir, projectName, "MyStaging");
            string mystaging_zip_file = "MyStaging.zip";
            if (!Directory.Exists(mystaging_file) && File.Exists(mystaging_zip_file))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(mystaging_zip_file, Path.Combine(outputDir, projectName));
            }
            // sln

            string sln_file = Path.Combine(outputDir, projectName, $"{projectName}.sln");
            if (!File.Exists(sln_file))
            {
                using (StreamWriter writer = new StreamWriter(File.Create(sln_file), System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                    writer.WriteLine("# Visual Studio 15>");
                    writer.WriteLine($"VisualStudioVersion = 15.0.26430.13");

                    Guid db_guid = Guid.NewGuid();
                    writer.WriteLine($"Project(\"{Guid.NewGuid()}\") = \"{projectName}.db\", \"{projectName}.db\\{projectName}.db.csproj\", \"{ db_guid}\"");
                    writer.WriteLine($"EndProject");

                    Guid staging_guid = Guid.NewGuid();
                    writer.WriteLine($"Project(\"{Guid.NewGuid()}\") = \"MyStaging\", \"MyStaging\\MyStaging.csproj\", \"{ staging_guid}\"");
                    writer.WriteLine($"EndProject");

                    writer.WriteLine("Global");
                    writer.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
                    writer.WriteLine("\t\tDebug|Any CPU = Debug|Any CPU");
                    writer.WriteLine("\t\tRelease|Any CPU = Release|Any CPU");
                    writer.WriteLine("\tEndGlobalSection");

                    writer.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
                    writer.WriteLine($"\t\t{db_guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                    writer.WriteLine($"\t\t{db_guid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                    writer.WriteLine($"\t\t{db_guid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                    writer.WriteLine($"\t\t{db_guid}.Release|Any CPU.Build.0 = Release|Any CPU");
                    writer.WriteLine($"\t\t{staging_guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                    writer.WriteLine($"\t\t{staging_guid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                    writer.WriteLine($"\t\t{staging_guid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                    writer.WriteLine($"\t\t{staging_guid}.Release|Any CPU.Build.0 = Release|Any CPU");
                    writer.WriteLine("\tEndGlobalSection");
                    writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
                    writer.WriteLine("\t\tHideSolutionNode = FALSE");
                    writer.WriteLine("\tEndGlobalSection");
                    writer.WriteLine("EndGlobal");
                }
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
                TableViewModel model = new TableViewModel() { name = dr["table_name"].ToString(), type = dr["type"].ToString() };
                tableList.Add(model);
            }, CommandType.Text, _sqltext);

            return tableList;
        }

    }
}
