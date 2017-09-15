using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.App.Models
{
    public class FieldInfo
    {
        public int Oid { get; set; }
        public string Field { get; set; }
        public int Length { get; set; }
        public string Comment { get; set; }
        public string RelType { get; set; }
        public string Db_type { get; set; }
        public string Data_Type { get; set; }
        public bool Is_identity { get; set; }
        public bool Is_array { get; set; }
        public bool Is_enum { get; set; }
        public bool Is_not_null { get; set; }
        public NpgsqlTypes.NpgsqlDbType PgDbType { get; set; }
    }
}
