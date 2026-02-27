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

        var response = await dbContext.GroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => new MyGroupResponse(
                m.Group.Id,
                m.Group.Name,
                m.Group.Description,
                m.Group.Members.Count))
            .FirstOrDefaultAsync(ct);

        await Send.OkAsync(Result.Success<MyGroupResponse?>(response), cancellation: ct);
    }
}
