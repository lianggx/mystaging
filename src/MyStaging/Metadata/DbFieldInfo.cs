using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Metadata
{
    public class DbFieldInfo
    {
        public int Oid { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public int Numeric_scale { get; set; }
        public string Comment { get; set; }
        public string CsType { get; set; }
        public string RelType { get; set; }
        public string DbType { get; set; }
        public bool Identity { get; set; }
        public bool IsArray { get; set; }
        public bool NotNull { get; set; }
    }
}
