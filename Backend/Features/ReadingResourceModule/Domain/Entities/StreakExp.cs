using PureTCOWebApp.Core.Models;
using System.Text.Json.Serialization;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

public class StreakExp : AuditableEntity
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public DateOnly StreakDateFrom { get; protected set; }
    public int Duration { get; protected set; }
    public int Exp { get; protected set; }

    [JsonIgnore]
    public ReadingResourceBase ReadingResource { get; protected set; }

#pragma warning disable CS8618
    public StreakExp() { }
#pragma warning restore CS8618

    public static StreakExp Create(
        int userId,
        DateOnly streakDateFrom,
        int duration,
        int exp
    )
    {
        return new StreakExp
        {
            UserId = userId,
            StreakDateFrom = streakDateFrom,
            Duration = duration,
            Exp = exp
        };
    }

    public void Update(DateOnly streakDateFrom, int duration, int exp)
    {
        StreakDateFrom = streakDateFrom;
        Duration = duration;
        Exp = exp;
    }
}
