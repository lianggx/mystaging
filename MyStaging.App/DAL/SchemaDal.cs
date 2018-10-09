using MyStaging;
using MyStaging.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MyStaging.App.DAL
{
    public class SchemaDal
    {
        /// <summary>
        ///  generate schema name list
        /// </summary>
        /// <returns></returns>
        public static List<string> Get_List()
        {
            List<string> schemaList = new List<string>();
            string[] notin = {
               "'geometry_columns'",
               "'raster_columns'",
               "'spatial_ref_sys'",
               "'raster_overviews'",
               "'us_gaz'",
               "'topology'",
               "'zip_lookup_all'",
               "'pg_toast'",
               "'pg_temp_1'",
               "'pg_toast_temp_1'",
               "'pg_catalog'",
               "'information_schema'",
               "'tiger'",
               "'tiger_data'"
            };
            string sql = $@"SELECT schema_name FROM information_schema.schemata WHERE SCHEMA_NAME NOT IN({string.Join(",", notin)}) ORDER BY SCHEMA_NAME; ";
            PgSqlHelper.ExecuteDataReader(dr =>
            {
                schemaList.Add(dr[0].ToString());
            }, CommandType.Text, sql);


            return schemaList;
        }
    }
}
