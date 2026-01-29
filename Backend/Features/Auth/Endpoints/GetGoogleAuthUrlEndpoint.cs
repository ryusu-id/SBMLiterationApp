using FastEndpoints;
using Microsoft.AspNetCore.WebUtilities;

namespace PureTCOWebApp.Features.Auth.Endpoints;

public class GetGoogleAuthUrlEndpoint : EndpointWithoutRequest<GetGoogleAuthUrlResponse>
{
    private readonly IConfiguration _configuration;

    public GetGoogleAuthUrlEndpoint(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void Configure()
    {
        Get("/api/auth/google/url");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var clientId = _configuration["GoogleAuthCredentials:ClientId"];
        var redirectUri =
            HttpContext.Request.Headers["Origin"].ToString() + "/auth/callback";

        // var redirectUri = _configuration["GoogleOAuth:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        // Generate a random state parameter for CSRF protection (frontend should validate this)
        var state = Guid.NewGuid().ToString("N");

        var queryParams = new Dictionary<string, string?>
        {
            { "client_id", clientId },
            { "redirect_uri", redirectUri },
            { "response_type", "code" },
            { "scope", "openid profile email" },
            { "state", state },
            { "access_type", "offline" },
            { "prompt", "consent" }
        };

        var authUrl = QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", queryParams);

        await Send.OkAsync(new GetGoogleAuthUrlResponse
        {
            AuthUrl = authUrl,
            State = state
        }, cancellation: ct);
    }
}

public class GetGoogleAuthUrlResponse
{
    public string AuthUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
