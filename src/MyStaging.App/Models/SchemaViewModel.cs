using System;
using System.Collections.Generic;
using System.Text;

namespace MyStaging.App.Models
{
    public class SchemaViewModel
    {
        public string Name { get; set; }
        public List<TableViewModel> Tables { get; set; }
    }
}
