using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.SubmissionEndpoints;

public record CreateSubmissionRequest(int Id, int GroupId);

public record CreateSubmissionResponse(
    int SubmissionId,
    int AssignmentId,
    int GroupId,
    string GroupName,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreateTime);

public class CreateSubmissionEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<CreateSubmissionRequest, ApiResponse<CreateSubmissionResponse>>
{
    public override void Configure()
    {
        Post("{id}/submissions");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(CreateSubmissionRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var assignment = await dbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == req.Id, ct);

        if (assignment is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Assignment", req.Id)));
            return;
        }

        var group = await dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == req.GroupId, ct);

        if (group is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Group", req.GroupId)));
            return;
        }

        var isMember = await dbContext.GroupMembers
            .AnyAsync(m => m.UserId == userId && m.GroupId == req.GroupId, ct);

        if (!isMember)
        {
            await Send.ResultAsync(TypedResults.Forbid());
            return;
        }

        var existingSubmission = await dbContext.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == req.Id && s.GroupId == req.GroupId, ct);

        if (existingSubmission is not null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)new Error("DuplicateSubmission", "Submission already exists for this group.")));
            return;
        }

        var submission = AssignmentSubmission.Create(req.Id, req.GroupId);

        await dbContext.AssignmentSubmissions.AddAsync(submission, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateSubmissionResponse(
                submission.Id,
                submission.AssignmentId,
                submission.GroupId,
                group.Name,
                submission.IsCompleted,
                submission.CompletedAt,
                submission.CreateTime)
        ), cancellation: ct);
    }
}
