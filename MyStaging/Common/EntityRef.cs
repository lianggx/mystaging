using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Common
{
    public class EntityRef
    {
        public string Field { get; set; }
        public NpgsqlTypes.NpgsqlDbType DbType { get; set; }
        public object Value { get; set; }
    }
}
