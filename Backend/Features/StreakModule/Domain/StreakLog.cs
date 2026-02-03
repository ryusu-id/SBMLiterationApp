using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.StreakModule.Domain.Events;

namespace PureTCOWebApp.Features.StreakModule.Domain;

public class StreakLog : AuditableEntity
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public DateOnly StreakDate { get; protected set; }

#pragma warning disable CS8618
    public StreakLog() { }
#pragma warning restore CS8618

    public static StreakLog Create(int userId, DateOnly streakDate)
    {
        var entity = new StreakLog
        {
            UserId = userId,
            StreakDate = streakDate
        };
        
        entity.Raise(new StreakLogCreatedEvent(entity));
        
        return entity;
    }
}
