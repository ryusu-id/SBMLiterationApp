using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.AssignmentModule.Domain;

public class Assignment : AuditableEntity
{
    public int Id { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public DateTime? DueDate { get; protected set; }

    public ICollection<AssignmentSubmission> Submissions { get; protected set; } = [];

    public Assignment() { }

    public static Assignment Create(string title, string? description, DateTime? dueDate) => new()
    {
        Title = title,
        Description = description,
        DueDate = dueDate
    };

    public void Update(string title, string? description, DateTime? dueDate)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
    }
}
