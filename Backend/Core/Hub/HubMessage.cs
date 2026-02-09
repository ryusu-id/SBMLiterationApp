using System.Text.Json;
using System.Text.Json.Serialization;

namespace PureTCOWebApp.Core.Hub;

public class HubMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public JsonElement Payload { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class SendMessageRequest
{
    [JsonPropertyName("targetUserId")]
    public string? TargetUserId { get; set; }

    [JsonPropertyName("targetSessionId")]
    public string? TargetSessionId { get; set; }

    [JsonPropertyName("message")]
    public HubMessage Message { get; set; } = new();
}
