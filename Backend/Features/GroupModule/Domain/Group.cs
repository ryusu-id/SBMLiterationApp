using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.GroupModule.Domain;

public class Group : AuditableEntity
{
    public int Id { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }

    public ICollection<GroupMember> Members { get; protected set; } = [];

    public Group() { }

    public static Group Create(string name, string? description) => new()
    {
        Name = name,
        Description = description
    };

    public void Rename(string name)
    {
        Name = name;
    }
}
