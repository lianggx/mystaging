using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.Metadata
{
    public class ProjectConfig
    {
        public string OutputDir { get; set; }
        public string ProjectName { get; set; }
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
        public GeneralMode Mode { get; set; }
    }
}
