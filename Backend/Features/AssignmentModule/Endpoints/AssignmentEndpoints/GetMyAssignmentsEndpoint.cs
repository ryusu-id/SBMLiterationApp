using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.AssignmentEndpoints;

public record MyAssignmentItem(
    int Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    int? SubmissionId,
    bool IsCompleted,
    DateTime? CompletedAt,
    int FileCount);

public record MyAssignmentsResponse(
    List<MyAssignmentItem> Active,
    List<MyAssignmentItem> Done,
    List<MyAssignmentItem> Missing);

public class GetMyAssignmentsEndpoint(
    ApplicationDbContext dbContext
) : EndpointWithoutRequest<ApiResponse<MyAssignmentsResponse>>
{
    public override void Configure()
    {
        Get("my");
        Group<AssignmentEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);

        var membership = await dbContext.GroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId, ct);

        if (membership is null)
        {
            await Send.OkAsync(Result.Success(new MyAssignmentsResponse([], [], [])), cancellation: ct);
            return;
        }

        var groupId = membership.GroupId;

        var now = DateTime.UtcNow;

        var items = await (
            from a in dbContext.Assignments.AsNoTracking()
            join sub in dbContext.AssignmentSubmissions.Where(s => s.GroupId == groupId)
                on a.Id equals sub.AssignmentId into subs
            from sub in subs.DefaultIfEmpty()
            select new MyAssignmentItem(
                a.Id,
                a.Title,
                a.Description,
                a.DueDate,
                sub == null ? (int?)null : sub.Id,
                sub != null && sub.IsCompleted,
                sub == null ? (DateTime?)null : sub.CompletedAt,
                sub == null ? 0 : sub.Files.Count()))
            .ToListAsync(ct);

        if (items.Count == 0)
        {
            await Send.OkAsync(Result.Success(new MyAssignmentsResponse([], [], [])), cancellation: ct);
            return;
        }

        // Done: marked as complete (regardless of due date)
        // Missing: past due date AND not marked complete (files attached or not doesn't matter)
        // Active: not past due date AND not marked complete
        var done = items
            .Where(a => a.IsCompleted)
            .OrderByDescending(a => a.DueDate)
            .ToList();

        var missing = items
            .Where(a => !a.IsCompleted && a.DueDate.HasValue && now > a.DueDate.Value)
            .OrderByDescending(a => a.DueDate)
            .ToList();

        var active = items
            .Where(a => !a.IsCompleted && (!a.DueDate.HasValue || now <= a.DueDate.Value))
            .OrderByDescending(a => a.DueDate)
            .ToList();

        await Send.OkAsync(Result.Success(new MyAssignmentsResponse(active, done, missing)), cancellation: ct);
    }
}
