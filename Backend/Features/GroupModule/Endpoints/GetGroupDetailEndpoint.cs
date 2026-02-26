using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public record GetGroupDetailRequest(int Id);

public record GroupMemberDetail(
    int UserId,
    string Fullname,
    string Nim,
    string? Email,
    string ProgramStudy,
    string Faculty,
    string GenerationYear,
    string? PictureUrl);

public record GetGroupDetailResponse(
    int Id,
    string Name,
    string? Description,
    DateTime CreateTime,
    DateTime? UpdateTime,
    List<GroupMemberDetail> Members);

public class GetGroupDetailEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetGroupDetailRequest, ApiResponse<GetGroupDetailResponse>>
{
    public override void Configure()
    {
        Get("{id}");
        Group<GroupEndpointGroup>();
        Roles("admin");
    }

    public override async Task HandleAsync(GetGroupDetailRequest req, CancellationToken ct)
    {
        var group = await dbContext.Groups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == req.Id, ct);

        if (group is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Group", req.Id)));
            return;
        }

        var members = group.Members
            .Select(m => new GroupMemberDetail(
                m.UserId,
                m.User.Fullname,
                m.User.Nim,
                m.User.Email,
                m.User.ProgramStudy,
                m.User.Faculty,
                m.User.GenerationYear,
                m.User.PictureUrl))
            .ToList();

        var response = new GetGroupDetailResponse(
            group.Id,
            group.Name,
            group.Description,
            group.CreateTime,
            group.UpdateTime,
            members);

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
