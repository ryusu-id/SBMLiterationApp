using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.UserXpModule.Domain;

public class UserExpSnapshot : AuditableEntity
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public int SnapshotSeq { get; protected set; }
    public int LastEventSeq { get; protected set; }
    public decimal Exp { get; protected set; }

#pragma warning disable CS8618
    protected UserExpSnapshot() { }
#pragma warning restore CS8618

    public static UserExpSnapshot Create(int userId, int snapshotSeq, int lastEventSeq, decimal exp)
    {
        return new UserExpSnapshot
        {
            UserId = userId,
            SnapshotSeq = snapshotSeq,
            LastEventSeq = lastEventSeq,
            Exp = exp
        };
    }

    public void Update(int lastEventSeq, decimal exp)
    {
        LastEventSeq = lastEventSeq;
        Exp = exp;
    }
}
