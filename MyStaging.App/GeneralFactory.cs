using MyStaging.App.DAL;
using MyStaging.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyStaging.App
{
    public class GeneralFactory
    {
        private string schemaPath = string.Empty;
        private string modelPath = string.Empty;
        private string dalPath = string.Empty;
        private string projectName = string.Empty;
        private string outputDir = string.Empty;

        public GeneralFactory(string outputdir, string projName)
        {
            if (string.IsNullOrEmpty(outputdir))
                throw new ArgumentNullException(nameof(outputdir));

            if (string.IsNullOrEmpty(projName))
                throw new ArgumentNullException(nameof(projName));

            outputDir = outputdir;
            projectName = projName;
        }

        public void Build()
        {
            var schemas = new SchemaDal().List();
            var plugins = FindPlugins(schemas);

            CreateDir();
            CreateCsproj(plugins);
            CreateEnums(plugins);
            CreateModels(schemas);
        }

        private List<PluginsViewModel> FindPlugins(List<SchemaViewModel> schemas)
        {
            var plugins = new List<PluginsViewModel>();
            foreach (var item in schemas)
            {
                if (item.Tables.FirstOrDefault(f => f.Name == "geometry_columns") != null)
                {
                    plugins.Add(new PluginsViewModel
                    {
                        Name = item.Name,
                        Mapper = "NpgsqlConnection.GlobalTypeMapper.UseLegacyPostgis();",
                        Package = "<PackageReference Include=\"Npgsql.LegacyPostgis\" Version=\"4.0.9\" />"
                    });
                }
            }

            return plugins;
        }

        private void CreateModels(List<SchemaViewModel> schemas)
        {
            foreach (var item in schemas)
            {
                Console.WriteLine("building：{0}", item.Name);
                foreach (var table in item.Tables)
                {
                    Console.WriteLine("{0}:{1}", table.Type, table.Name);
                    TablesDal td = new TablesDal(projectName, modelPath, schemaPath, dalPath, item.Name, table);
                    td.Create();
                }
            }
        }

        private void CreateEnums(List<PluginsViewModel> plugins)
        {
            var enumsDal = new EnumsDal(Path.Combine(outputDir, projectName + ".db"), modelPath, projectName, plugins);
            enumsDal.Generate();
        }

        private void CreateDir()
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

        private void CreateCsproj(List<PluginsViewModel> plugins)
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
                writer.WriteLine("\t\t<PackageReference Include=\"MyStaging\" Version=\"2.*\" />");
                foreach (var item in plugins)
                {
                    writer.WriteLine($"\t\t{item.Package}");
                }
                writer.WriteLine("\t</ItemGroup>");
                writer.WriteLine("</Project>");
            }
        }
    }
}
