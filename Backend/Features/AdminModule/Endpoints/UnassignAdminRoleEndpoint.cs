using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.AdminModule.Endpoints;

public record UnassignAdminRoleRequest(int UserId);

public class UnassignAdminRoleEndpoint(UserManager<User> userManager)
    : Endpoint<UnassignAdminRoleRequest>
{
    public override void Configure()
    {
        Get("{UserId}/unassign");
        Group<AdminEndpointGroup>();
    }

    public override async Task HandleAsync(UnassignAdminRoleRequest req, CancellationToken ct)
    {
        var currentUserId = int.Parse(User.FindFirst("sub")!.Value);

        if (currentUserId == req.UserId)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(Result.Failure(new Error("UnassignAdminRole.SelfUnassign", "Cannot unassign admin role from yourself"))));
            return;
        }

        var user = await userManager.FindByIdAsync(req.UserId.ToString());
        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await userManager.RemoveFromRoleAsync(user, "admin");

        if (!result.Succeeded)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description))));
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
