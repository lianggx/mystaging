using MyStaging.Common;
using MyStaging.Core;
using MyStaging.Metadata;
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
        private readonly TableInfo table;
        private readonly GeneralConfig config;
        #endregion

        public EntityGeneral(GeneralConfig config, TableInfo table)
        {
            this.config = config;
            this.table = table;
        }

        public void Create()
        {
            string _classname = CreateName() + "Model";
            string _fileName = $"{config.ModelPath}/{_classname}.cs";
            using StreamWriter writer = new StreamWriter(File.Create(_fileName), System.Text.Encoding.UTF8);
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Linq;");
            writer.WriteLine("using Newtonsoft.Json;");
            writer.WriteLine("using Newtonsoft.Json.Linq;");
            writer.WriteLine("using NpgsqlTypes;");
            writer.WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
            writer.WriteLine("using System.ComponentModel.DataAnnotations;");
            writer.WriteLine();
            writer.WriteLine($"namespace {config.ProjectName}.Model");
            writer.WriteLine("{");
            writer.WriteLine($"\t[Table(name: \"{this.table.Name}\", Schema = \"{table.Schema}\")]");
            writer.WriteLine($"\tpublic partial class {_classname}");
            writer.WriteLine("\t{");

            foreach (var fi in table.Fields)
            {
                if (!string.IsNullOrEmpty(fi.Comment))
                {
                    writer.WriteLine("\t\t/// <summary>");
                    writer.WriteLine($"\t\t/// {fi.Comment}");
                    writer.WriteLine("\t\t/// </summary>");
                }

                if (fi.Identity)
                    writer.WriteLine("\t\t[Key]");
                if (fi.NotNull && fi.RelType == "string" && !fi.Identity)
                    writer.WriteLine("\t\t[Required]");
                if (fi.RelType == "string" && (fi.Length != 0 && fi.Length != 255))
                    writer.WriteLine($"\t\t[StringLength({fi.Length})]");
                else if (fi.Numeric_scale > 0)
                {
                    writer.WriteLine($"\t\t[StringLength({fi.Length}, MinimumLength = {fi.Numeric_scale})]");
                }
                if (PgsqlType.ContrastType(fi.DbType) == null)
                {
                    writer.WriteLine($"\t\t[DataType(\"{fi.DbType}\")]");
                }

                //writer.WriteLine($"\t\tpublic {_type} {fi.Name} {{ get; set; }}");
                writer.WriteLine($"\t\tpublic {fi.RelType} {fi.Name} {{ get; set; }}");
            }
            writer.WriteLine("\t}");
            writer.WriteLine("}");
            writer.Flush();
        }

        private string CreateName(string separator = "")
        {
            string className;
            if (table.Schema == "public")
            {
                className = separator + table.Name.ToUpperPascal();
            }
            else
            {
                className = $"{table.Schema.ToUpperPascal()}{separator}{table.Name.ToUpperPascal()}";
            }

            return className;
        }
    }
}
