using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.SubmissionEndpoints;

public record GetMySubmissionRequest(int AssignmentId);

public class GetMySubmissionEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetMySubmissionRequest, ApiResponse<SubmissionDetail?>>
{
    public override void Configure()
    {
        Get("{assignmentId}/submission/my");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(GetMySubmissionRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var groupId = await dbContext.GroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.GroupId)
            .FirstOrDefaultAsync(ct);

        if (groupId == default)
        {
            await Send.OkAsync(Result.Success<SubmissionDetail?>(null), cancellation: ct);
            return;
        }

        var response = await dbContext.AssignmentSubmissions
            .Where(s => s.AssignmentId == req.AssignmentId && s.GroupId == groupId)
            .Select(s => new SubmissionDetail(
                s.Id,
                s.AssignmentId,
                s.GroupId,
                s.Group.Name,
                s.IsCompleted,
                s.CompletedAt,
                s.CreateTime,
                s.Files.Select(f => new SubmissionFileDetail(
                    f.Id,
                    f.FileName,
                    f.FileUri,
                    f.ExternalLink,
                    f.UploadedByUserId,
                    f.UploadedBy.Fullname,
                    f.CreateTime)).ToList()))
            .FirstOrDefaultAsync(ct);

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
