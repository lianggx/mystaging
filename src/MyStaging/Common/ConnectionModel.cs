using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace MyStaging.Common
{
    public class ConnectionModel
    {
        public bool ReadOnly { get; set; }

        public string ConnectionString { get; set; }

        public long Used { get; internal set; }

        public long Error { get; internal set; }
    }
}
