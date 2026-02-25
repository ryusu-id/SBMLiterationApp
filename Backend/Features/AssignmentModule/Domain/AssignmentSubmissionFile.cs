using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Domain;

public class AssignmentSubmissionFile
{
    public int Id { get; protected set; }
    public int AssignmentSubmissionId { get; protected set; }
    public int UploadedByUserId { get; protected set; }
    public string FileName { get; protected set; } = string.Empty;
    public string? FileUri { get; protected set; }
    public string? ExternalLink { get; protected set; }
    public DateTime UploadedAt { get; protected set; }

    public AssignmentSubmission Submission { get; protected set; } = null!;
    public User UploadedBy { get; protected set; } = null!;

#pragma warning disable CS8618
    protected AssignmentSubmissionFile() { }
#pragma warning restore CS8618

    public static AssignmentSubmissionFile Create(
        int submissionId,
        int userId,
        string fileName,
        string? fileUri,
        string? externalLink) => new()
    {
        AssignmentSubmissionId = submissionId,
        UploadedByUserId = userId,
        FileName = fileName,
        FileUri = fileUri,
        ExternalLink = externalLink,
        UploadedAt = DateTime.UtcNow
    };
}
