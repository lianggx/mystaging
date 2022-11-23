using Microsoft.Extensions.Logging;
using MyStaging.Interface;
using System;

namespace MyStaging.Metadata
{
    public class StagingOptions
    {
        public StagingOptions(string name, string master, params string[] slaves)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(master))
                throw new ArgumentNullException(nameof(master));

            this.Master = master;
            this.Slaves = slaves;
            this.Name = name;
        }

        public string Name { get; }
        public ILogger Logger { get; set; }
        public IStagingConnection Connection { get; set; }
        public ProviderType Provider { get; set; }
        public string Master { get; set; }
        public string[] Slaves { get; set; }
    }
}
