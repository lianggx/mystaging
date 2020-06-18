using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Gen.Tool.Models
{
    public class FieldInfo
    {
        public int Oid { get; set; }
        public string Field { get; set; }
        public int Length { get; set; }
        public string Comment { get; set; }
        public string CsType { get; set; }
        public string RelType { get; set; }
        public string DbType { get; set; }
        public string DataType { get; set; }
        public bool Identity { get; set; }
        public bool IsArray { get; set; }
        public bool IsEnum { get; set; }
        public bool NotNull { get; set; }
    }
}
