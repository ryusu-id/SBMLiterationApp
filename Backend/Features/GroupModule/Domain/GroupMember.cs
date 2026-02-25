using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.GroupModule.Domain;

public class GroupMember
{
    public int Id { get; protected set; }
    public int GroupId { get; protected set; }
    public int UserId { get; protected set; }

    public Group Group { get; protected set; } = null!;
    public User User { get; protected set; } = null!;

#pragma warning disable CS8618
    protected GroupMember() { }
#pragma warning restore CS8618

    public static GroupMember Create(int groupId, int userId) => new()
    {
        GroupId = groupId,
        UserId = userId
    };
}
