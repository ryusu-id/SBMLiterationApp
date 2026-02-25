using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints;

public class EmptyRequest { }

public record MyAssignmentItem(
    int AssignmentId,
    string Title,
    string? Description,
    DateTime? DueDate,
    int SubmissionId,
    bool IsCompleted,
    DateTime? CompletedAt,
    int FileCount);

public class GetMyAssignmentsEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : EndpointWithoutRequest<ApiResponse<List<MyAssignmentItem>>>
{
    public override void Configure()
    {
        Get("my");
        Group<AssignmentEndpointGroup>();
        Roles("participant");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        // Find the user's group
        var membership = await dbContext.GroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId, ct);

        if (membership is null)
        {
            // User not in any group — return empty
            await Send.OkAsync(Result.Success(new List<MyAssignmentItem>()), cancellation: ct);
            return;
        }

        var groupId = membership.GroupId;

        // Get all assignments
        var assignments = await dbContext.Assignments
            .AsNoTracking()
            .OrderByDescending(a => a.CreateTime)
            .ToListAsync(ct);

        if (assignments.Count == 0)
        {
            await Send.OkAsync(Result.Success(new List<MyAssignmentItem>()), cancellation: ct);
            return;
        }

        var assignmentIds = assignments.Select(a => a.Id).ToList();

        // Get existing submissions for this group
        var existingSubmissions = await dbContext.AssignmentSubmissions
            .Include(s => s.Files)
            .Where(s => s.GroupId == groupId && assignmentIds.Contains(s.AssignmentId))
            .ToListAsync(ct);

        var submissionByAssignment = existingSubmissions.ToDictionary(s => s.AssignmentId);

        // Create missing submissions
        var missingAssignmentIds = assignmentIds.Except(submissionByAssignment.Keys).ToList();
        if (missingAssignmentIds.Count > 0)
        {
            var newSubmissions = missingAssignmentIds
                .Select(aId => AssignmentSubmission.Create(aId, groupId))
                .ToList();

            await dbContext.AssignmentSubmissions.AddRangeAsync(newSubmissions, ct);

            try
            {
                var saveResult = await unitOfWork.SaveChangesAsync(ct);
                if (saveResult.IsSuccess)
                {
                    foreach (var s in newSubmissions)
                        submissionByAssignment[s.AssignmentId] = s;
                }
                else
                {
                    // Unique constraint hit (race) — re-fetch
                    var refetched = await dbContext.AssignmentSubmissions
                        .Include(s => s.Files)
                        .Where(s => s.GroupId == groupId && missingAssignmentIds.Contains(s.AssignmentId))
                        .ToListAsync(ct);
                    foreach (var s in refetched)
                        submissionByAssignment[s.AssignmentId] = s;
                }
            }
            catch
            {
                // Re-fetch on any failure
                var refetched = await dbContext.AssignmentSubmissions
                    .Include(s => s.Files)
                    .Where(s => s.GroupId == groupId && missingAssignmentIds.Contains(s.AssignmentId))
                    .ToListAsync(ct);
                foreach (var s in refetched)
                    submissionByAssignment[s.AssignmentId] = s;
            }
        }

        var response = assignments
            .Select(a =>
            {
                if (!submissionByAssignment.TryGetValue(a.Id, out var sub)) return null;
                return new MyAssignmentItem(
                    a.Id, a.Title, a.Description, a.DueDate,
                    sub.Id, sub.IsCompleted, sub.CompletedAt, sub.Files.Count);
            })
            .OfType<MyAssignmentItem>()
            .ToList();

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}
