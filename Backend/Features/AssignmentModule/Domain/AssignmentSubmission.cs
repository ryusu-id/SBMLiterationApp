using PureTCOWebApp.Features.GroupModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Domain;

public class AssignmentSubmission
{
    public int Id { get; protected set; }
    public int AssignmentId { get; protected set; }
    public int GroupId { get; protected set; }
    public bool IsCompleted { get; protected set; }
    public DateTime? CompletedAt { get; protected set; }
    public DateTime CreateTime { get; protected set; }

    public Assignment Assignment { get; protected set; } = null!;
    public Group Group { get; protected set; } = null!;
    public ICollection<AssignmentSubmissionFile> Files { get; protected set; } = [];

#pragma warning disable CS8618
    protected AssignmentSubmission() { }
#pragma warning restore CS8618

    public static AssignmentSubmission Create(int assignmentId, int groupId) => new()
    {
        AssignmentId = assignmentId,
        GroupId = groupId,
        IsCompleted = false,
        CreateTime = DateTime.UtcNow
    };

    public void MarkAsComplete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsIncomplete()
    {
        IsCompleted = false;
        CompletedAt = null;
    }
}
