using System.Text.Json;
using System.Text.Json.Serialization;

namespace PureTCOWebApp.Core.JsonConverter;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string _format = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {

#pragma warning disable
        
        return DateTime.ParseExact(reader.GetString(), _format, null);
#pragma warning restore
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}
