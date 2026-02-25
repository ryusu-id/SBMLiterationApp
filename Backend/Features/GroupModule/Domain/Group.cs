namespace PureTCOWebApp.Features.GroupModule.Domain;

public class Group
{
    public int Id { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public DateTime CreateTime { get; protected set; }
    public DateTime? UpdateTime { get; protected set; }

    public ICollection<GroupMember> Members { get; protected set; } = [];

#pragma warning disable CS8618
    protected Group() { }
#pragma warning restore CS8618

    public static Group Create(string name, string? description) => new()
    {
        Name = name,
        Description = description,
        CreateTime = DateTime.UtcNow
    };

    public void Rename(string name)
    {
        Name = name;
        UpdateTime = DateTime.UtcNow;
    }
}
