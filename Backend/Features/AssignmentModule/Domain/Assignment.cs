namespace PureTCOWebApp.Features.AssignmentModule.Domain;

public class Assignment
{
    public int Id { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public DateTime? DueDate { get; protected set; }
    public DateTime CreateTime { get; protected set; }
    public DateTime? UpdateTime { get; protected set; }

    public ICollection<AssignmentSubmission> Submissions { get; protected set; } = [];

#pragma warning disable CS8618
    protected Assignment() { }
#pragma warning restore CS8618

    public static Assignment Create(string title, string? description, DateTime? dueDate) => new()
    {
        Title = title,
        Description = description,
        DueDate = dueDate,
        CreateTime = DateTime.UtcNow
    };

    public void Update(string title, string? description, DateTime? dueDate)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        UpdateTime = DateTime.UtcNow;
    }
}
