using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace PureTCOWebApp.Features.Auth.Domain;

[JsonConverter(typeof(UserJsonConverter))]
public class User : IdentityUser<int>
{
    #pragma warning disable 
    public User()
    {
    }
    #pragma warning restore
    public string Nim { get; set; }
    public string Fullname { get; set; }
    public string ProgramStudy { get; set; }
    public string Faculty { get; set; }
    public string GenerationYear { get; set; }
    public string? PictureUrl { get; set; }
}

public class UserJsonConverter : JsonConverter<User>
{
    public override User Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For deserialization, use default behavior
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var user = JsonSerializer.Deserialize<User>(jsonDocument.RootElement.GetRawText(), 
            new JsonSerializerOptions { Converters = { } });
        return user ?? new User();
    }

    public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // // Include the Id property from IdentityUser
        // var idPropertyName = options.PropertyNamingPolicy?.ConvertName("Id") ?? "Id";
        // writer.WritePropertyName(idPropertyName);
        // JsonSerializer.Serialize(writer, value.Id, options);
        
        // Only serialize properties declared in the User class itself
        var userProperties = typeof(User).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        foreach (var prop in userProperties)
        {
            var propValue = prop.GetValue(value);
            var propertyName = options.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name;
            
            writer.WritePropertyName(propertyName);
            JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
        }
        
        writer.WriteEndObject();
    }
}