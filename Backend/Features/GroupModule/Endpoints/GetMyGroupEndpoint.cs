using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public record MyGroupResponse(int Id, string Name, string? Description, int MemberCount);

public class GetMyGroupEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<ApiResponse<MyGroupResponse?>>
{
    public override void Configure()
    {
        Get("my");
        Group<GroupEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var membership = await dbContext.GroupMembers
            .AsNoTracking()
            .Include(m => m.Group)
                .ThenInclude(g => g.Members)
            .FirstOrDefaultAsync(m => m.UserId == userId, ct);

        if (membership is null)
        {
            await Send.OkAsync(Result.Success<MyGroupResponse?>(null), cancellation: ct);
            return;
        }

        var group = membership.Group;
        var response = new MyGroupResponse(
            group.Id,
            group.Name,
            group.Description,
            group.Members.Count);

        await Send.OkAsync(Result.Success<MyGroupResponse?>(response), cancellation: ct);
    }
}
