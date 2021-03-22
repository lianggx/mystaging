using IdentityHost.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace IdentityHost.Extensions
{
    public class JsonSerializerExtension
    {
        public static JsonSerializerOptions JsonOptions
        {
            get
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonBooleanConverter());
                options.Converters.Add(new JsonUnixTimeConverter());
                options.Converters.Add(new JsonStringEnumConverter());
                options.PropertyNamingPolicy = new JsonLowerCaseNamingPolicy();
                options.PropertyNameCaseInsensitive = true;
                options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                return options;
            }
        }
    }
}
