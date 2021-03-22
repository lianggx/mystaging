using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdentityHost.Converters
{
    public class JsonBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetBoolean();
        }
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            uint reValue = Convert.ToUInt32(Convert.ToBoolean(value));
            writer.WriteNumberValue(reValue);
        }
    }
}
