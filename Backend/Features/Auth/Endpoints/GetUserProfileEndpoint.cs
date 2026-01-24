using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.Auth.Endpoints;

public class GetUserProfileResponse
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string Nim { get; set; } = string.Empty;
    public string ProgramStudy { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public int? GenerationYear { get; set; }
    public IList<string> Roles { get; set; } = [];
}

public class GetUserProfileEndpoint : EndpointWithoutRequest<GetUserProfileResponse>
{
    private readonly UserManager<User> _userManager;

    public GetUserProfileEndpoint(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public override void Configure()
    {
        Get("/api/auth/profile");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await _userManager.FindByIdAsync(userIdClaim);

        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var roles = await _userManager.GetRolesAsync(user);

        await Send.OkAsync(new GetUserProfileResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Nim = user.Nim,
            ProgramStudy = user.ProgramStudy,
            Faculty = user.Faculty,
            GenerationYear = user.GenerationYear,
            Roles = roles
        }, ct);
    }
}
