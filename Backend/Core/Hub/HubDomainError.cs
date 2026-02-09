using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Core.Hub;

public static class HubDomainError
{
    public static Error ConnectionFailed => new(nameof(ConnectionFailed), "Failed to connect to Hub service");
    public static Error SendFailed => new(nameof(SendFailed), "Failed to send message to Hub service");
    public static Error InvalidConfiguration => new(nameof(InvalidConfiguration), "Hub service is not properly configured");
}
