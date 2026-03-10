using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public record GroupMemberResponse(
    string Fullname,
    string Nim,
    string ProgramStudy,
    string Campus,
    string Class,
    string? PictureUrl
);

public record MyGroupResponse(
    int Id,
    string Name,
    string? Description,
    IEnumerable<GroupMemberResponse> Members
);

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
                m.Group.Members.Select(e => new GroupMemberResponse(
                    e.User.Fullname,
                    e.User.Nim,
                    e.User.ProgramStudy,
                    e.User.Faculty,
                    e.User.GenerationYear,
                    e.User.PictureUrl
                ))
            ))
            .FirstOrDefaultAsync(ct);

        await Send.OkAsync(Result.Success<MyGroupResponse?>(response), cancellation: ct);
    }
}
