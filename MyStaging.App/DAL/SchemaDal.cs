using MyStaging.App.Models;
using MyStaging.Helpers;
using System;
using System.Collections.Generic;
using System.Data;

namespace MyStaging.App.DAL
{
    public class SchemaDal
    {
        public SchemaDal()
        {
            Filters.AddRange(new string[]{
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
            });
        }

        public List<string> Filters { get; set; } = new List<string>();

        public List<SchemaViewModel> List()
        {
            string[] filters = new string[this.Filters.Count];
            for (int i = 0; i < Filters.Count; i++)
            {
                filters[i] = $"'{Filters[i]}'";
            }

            List<SchemaViewModel> schemas = new List<SchemaViewModel>();
            string sql = $@"SELECT schema_name FROM information_schema.schemata WHERE SCHEMA_NAME NOT IN({string.Join(",", filters)}) ORDER BY SCHEMA_NAME; ";
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                var schema = new SchemaViewModel { Name = dr[0].ToString() };
                schemas.Add(schema);
            }, CommandType.Text, sql);

            foreach (var item in schemas)
            {
                Console.WriteLine("正在生成模式：{0}", item.Name);
                item.Tables = GetTables(item.Name);
            }

            return schemas;
        }

        private List<TableViewModel> GetTables(string schema)
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
