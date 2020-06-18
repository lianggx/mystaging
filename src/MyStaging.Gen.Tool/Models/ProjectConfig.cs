using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Gen.Tool.Models
{
    public class ProjectConfig
    {
        public string OutputDir { get; set; }
        public string ProjectName { get; set; }
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
    }
}
