using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Schemas
{
    public class SchemaModel
    {
        public string FieldName { get; set; }
        public NpgsqlDbType DbType { get; set; }
        public int Size { get; set; }
        public Type SpecificType { get; set; }
    }
}
