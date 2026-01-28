using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.Auth.Endpoints;

public class GoogleAuthCallbackEndpoint : Endpoint<GoogleAuthCallbackRequest, GoogleAuthCallbackResponse>
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleAuthCallbackEndpoint(
        IConfiguration configuration,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _httpClientFactory = httpClientFactory;
    }

    public override void Configure()
    {
        Post("/api/auth/google/callback");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GoogleAuthCallbackRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Code))
        {
            await Send.ErrorsAsync();
            return;
        }

        try
        {
            // Exchange authorization code for access token
            var tokenResponse = await ExchangeCodeForTokenAsync(req.Code, ct);
            
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                await Send.ResultAsync(
                    TypedResults.BadRequest(new GoogleAuthCallbackResponse
                    {
                        Success = false,
                        Message = "Failed to exchange authorization code for access token"
                    }));
                return;
            }

            // Get user info from Google
            var userInfo = await GetGoogleUserInfoAsync(tokenResponse.AccessToken, ct);
            
            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                await Send.ResultAsync(
                    TypedResults.BadRequest(new GoogleAuthCallbackResponse
                    {
                        Success = false,
                        Message = "Failed to retrieve user information from Google"
                    }));
                return;
            }

            // Find or create user
            var user = await _userManager.FindByEmailAsync(userInfo.Email);
            
            if (user == null)
            {
                user = new User
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    Fullname = userInfo.Name,
                    Nim = "", // Set default or from additional data
                    ProgramStudy = "", // Set default or from additional data
                    Faculty = "", // Set default or from additional data
                    GenerationYear = "", // Set default or from additional data
                    PictureUrl = userInfo.Picture,
                };

                var createResult = await _userManager.CreateAsync(user);
                
                if (!createResult.Succeeded)
                {
                    await Send.ResultAsync(
                        TypedResults.BadRequest(new GoogleAuthCallbackResponse
                        {
                            Success = false,
                            Message = $"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}"
                        }));
                    return;
                }

                // Add Google login info
                await _userManager.AddLoginAsync(user, new UserLoginInfo(
                    "Google",
                    userInfo.Sub,
                    "Google"
                ));
            }
            else
            {
                // Check if Google login is already associated
                var logins = await _userManager.GetLoginsAsync(user);
                var googleLogin = logins.FirstOrDefault(l => l.LoginProvider == "Google");
                user.PictureUrl = userInfo.Picture;
                
                if (googleLogin == null)
                {
                    await _userManager.AddLoginAsync(user, new UserLoginInfo(
                        "Google",
                        userInfo.Sub,
                        "Google"
                    ));
                }
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Check if user is locked or disabled
            if (await _userManager.IsLockedOutAsync(user))
            {
                await Send.ResultAsync(
                    TypedResults.BadRequest(new GoogleAuthCallbackResponse
                    {
                        Success = false,
                        Message = "Account is locked or disabled. Please contact administrator."
                    }));
                return;
            }

            // Generate JWT tokens
            var (accessToken, refreshToken) = await _jwtTokenService.GenerateTokensAsync(user, roles);

            // Return tokens
            await Send.OkAsync(new GoogleAuthCallbackResponse
            {
                Success = true,
                Message = "Authentication successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during Google OAuth callback");
            await Send.ResultAsync(TypedResults.BadRequest(new GoogleAuthCallbackResponse
            {
                Success = false,
                Message = "An internal error occurred during authentication"
            }));
        }
    }

    private async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code, CancellationToken ct)
    {
        var clientId = _configuration["GoogleAuthCredentials:ClientId"];
        var clientSecret = _configuration["GoogleAuthCredentials:ClientSecret"];
        // var redirectUri = _configuration["GoogleOAuth:RedirectUri"];
        var redirectUri =
            HttpContext.Request.Headers["Origin"].ToString() + "/auth/callback";

        var client = _httpClientFactory.CreateClient();
        
        var requestData = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId! },
            { "client_secret", clientSecret! },
            { "redirect_uri", redirectUri! },
            { "grant_type", "authorization_code" }
        };

        Logger.LogError("Response: {Response}", JsonSerializer.Serialize(requestData));
        var response = await client.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(requestData),
            ct
        );

        if (!response.IsSuccessStatusCode)
        {
            Logger.LogError("Failed to exchange code for token. Status: {StatusCode}", response.StatusCode);
            Logger.LogError("Response: {Response}", await response.Content.ReadAsStringAsync(ct));
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<GoogleTokenResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task<GoogleUserInfo?> GetGoogleUserInfoAsync(string accessToken, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo", ct);

        if (!response.IsSuccessStatusCode)
        {
            Logger.LogError("Failed to get user info. Status: {StatusCode}", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}

public class GoogleAuthCallbackRequest
{
    public string? Code { get; set; }
}

public class GoogleAuthCallbackResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}

public class GoogleUserInfo
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
}
