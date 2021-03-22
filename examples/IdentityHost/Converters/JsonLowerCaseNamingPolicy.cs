using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IdentityHost.Converters
{
    public class JsonLowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
}
