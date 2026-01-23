using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.AdminModule.Endpoints;

public record AssignAdminRoleRequest(int UserId);

public class AssignAdminRoleEndpoint(UserManager<User> userManager)
    : Endpoint<AssignAdminRoleRequest, ApiResponse>
{
    public override void Configure()
    {
        Get("{UserId}/assign");
        Group<AdminEndpointGroup>();
    }

    public override async Task HandleAsync(AssignAdminRoleRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.UserId.ToString());
        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await userManager.AddToRoleAsync(user, "admin");
        if (!result.Succeeded)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description))));
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
