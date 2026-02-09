using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Core.Hub;

/// <summary>
/// Client for sending messages to the WebSocket Hub service
/// </summary>
public interface IHubClient
{
    Task<Result> SendToUserAsync(string userId, object message, string messageType = "notification", CancellationToken ct = default);
    Task<Result> SendToSessionAsync(string sessionId, object message, string messageType = "notification", CancellationToken ct = default);
    Task<Result> BroadcastAsync(object message, string messageType = "notification", CancellationToken ct = default);
}

public class HubClient : IHubClient
{
    private readonly HttpClient _httpClient;
    private readonly HubSettings _settings;

    public HubClient(HttpClient httpClient, IOptions<HubSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
    }

    public async Task<Result> SendToUserAsync(string userId, object message, string messageType = "notification", CancellationToken ct = default)
    {
        var request = new SendMessageRequest
        {
            TargetUserId = userId,
            Message = new HubMessage
            {
                Type = messageType,
                Payload = JsonSerializer.SerializeToElement(message),
                Timestamp = DateTime.UtcNow
            }
        };

        return await SendRequestAsync("/api/send", request, ct);
    }

    public async Task<Result> SendToSessionAsync(string sessionId, object message, string messageType = "notification", CancellationToken ct = default)
    {
        var request = new SendMessageRequest
        {
            TargetSessionId = sessionId,
            Message = new HubMessage
            {
                Type = messageType,
                Payload = JsonSerializer.SerializeToElement(message),
                Timestamp = DateTime.UtcNow
            }
        };

        return await SendRequestAsync("/api/send", request, ct);
    }

    public async Task<Result> BroadcastAsync(object message, string messageType = "notification", CancellationToken ct = default)
    {
        var hubMessage = new HubMessage
        {
            Type = messageType,
            Payload = JsonSerializer.SerializeToElement(message),
            Timestamp = DateTime.UtcNow
        };

        return await SendRequestAsync("/api/broadcast", hubMessage, ct);
    }

    private async Task<Result> SendRequestAsync<T>(string endpoint, T payload, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, payload, ct);
            
            if (response.IsSuccessStatusCode)
            {
                return Result.Success("Message sent successfully");
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return new Error("HUB_REQUEST_FAILED", $"Failed to send message: {response.StatusCode} - {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            return new Error("HUB_CONNECTION_ERROR", $"Failed to connect to Hub service: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new Error("HUB_ERROR", $"Unexpected error: {ex.Message}");
        }
    }
}
