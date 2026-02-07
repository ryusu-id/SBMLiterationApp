using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.UserXpModule.Domain.Events;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.UserXpModule.Domain;

public class UserExpEvent : AuditableEntity
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public decimal Exp { get; protected set; }
    public string EventName { get; protected set; }
    public int RefId { get; protected set; }

#pragma warning disable CS8618
    protected UserExpEvent() { }
#pragma warning restore CS8618

    public static UserExpEvent Create(int userId, decimal exp, string eventName, int refId)
    {
        var entity = new UserExpEvent
        {
            UserId = userId,
            Exp = exp,
            EventName = eventName,
            RefId = refId
        };
        
        entity.Raise(new UserExpCreatedEvent(entity));
        
        return entity;
    }

    public enum ExpEventType
    {
        ReadingExp,
        DailyReadsExp,
        StreakExp,
        BookCompleted,
        RecommendedBookCompleted
    }

    public ExpEventType EventType => Enum.Parse<ExpEventType>(EventName);
}
