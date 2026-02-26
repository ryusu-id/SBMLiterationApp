using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.GroupModule.Domain;

public class GroupMember : AuditableEntity
{
    public int Id { get; protected set; }
    public int GroupId { get; protected set; }
    public int UserId { get; protected set; }

    public Group Group { get; protected set; } = null!;
    public User User { get; protected set; } = null!;

    public GroupMember() { }

    public static GroupMember Create(int groupId, int userId) => new()
    {
        GroupId = groupId,
        UserId = userId
    };
}
