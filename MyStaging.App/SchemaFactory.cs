using MyStaging.App.DAL;
using MyStaging.Helpers;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MyStaging.App
{
    public class SchemaFactory
    {
        private static string modelpath = string.Empty;
        private static string dalpath = string.Empty;
        private static string projectName = string.Empty;
        public static void Start(string _projectName)
        {
            projectName = _projectName;
            CreateDir();
            EnumsDal.Generate(projectName);

            List<string> schemaList = SchemaDal.Get_List();
            foreach (var schemaName in schemaList)
            {
                List<string> tableList = GetTables(schemaName);
                foreach (var tableName in tableList)
                {
                    //if (tableName != "group_guser")
                    //    continue;
                    TablesDal td = new TablesDal(projectName, modelpath, dalpath, schemaName, tableName);
                    td.Generate();
                    //    break;
                }
                //   break;
            }

        }

        protected static void CreateDir()
        {
            modelpath = $"{projectName}/Model/Build";
            dalpath = $"{projectName}/DAL/Build";
            string[] ps = { projectName, modelpath, dalpath };
            for (int i = 0; i < ps.Length; i++)
            {
                if (!Directory.Exists(ps[i]))
                    Directory.CreateDirectory(ps[i]);
            }
        }

        protected static List<string> GetTables(string schema)
        {
            string _sqltext = $"SELECT table_name FROM INFORMATION_SCHEMA.tables WHERE table_schema='{schema}' AND table_type='BASE TABLE';";
            List<string> tableList = new List<string>();
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                tableList.Add(dr[0].ToString());
            }, CommandType.Text, _sqltext);

            return tableList;
        }

    }
}
