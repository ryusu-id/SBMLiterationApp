using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.Auth.Endpoints;

public class SiteEndpoint : EndpointWithoutRequest<ApiResponse<User>>
{
    private readonly UserManager<User> _userManager;

    public SiteEndpoint(
        UserManager<User> userManager
    )
    {
        _userManager = userManager;
    }

    public override void Configure()
    {
        Get("auth/site");
        Group<GlobalApiEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(Result.Success(user), ct);
    }
}
