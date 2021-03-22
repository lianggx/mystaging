using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdentityHost.Converters
{
    public class JsonUnixTimeConverter : JsonConverter<DateTime>
    {
        private static DateTime Greenwich_Mean_Time = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
        private const int Limit = 10000;

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                var unixTime = reader.GetInt64();
                var dt = new DateTime(Greenwich_Mean_Time.Ticks + unixTime * Limit);
                return dt;
            }
            else
            {
                return reader.GetDateTime();
            }
        }
        public override void Write(Utf8JsonWriter writer, DateTime dateTime, JsonSerializerOptions options)
        {
            var unixTime = (dateTime - Greenwich_Mean_Time).Ticks / Limit;
            writer.WriteNumberValue(unixTime);
        }
    }
}
