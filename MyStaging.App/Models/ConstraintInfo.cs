using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.App.Models
{
    public class ConstraintInfo
    {
        public string conname { get; set; }
        public string contype { get; set; }
        public string ref_column { get; set; }
        public string table_name { get; set; }
        public string nspname { get; set; }
    }
}
