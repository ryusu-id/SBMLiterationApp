namespace PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

public class StreakLog
{
    public int Id { get; protected set; }
    public int UserId { get; protected set; }
    public DateOnly StreakDate { get; protected set; }

#pragma warning disable CS8618
    public StreakLog() { }
#pragma warning restore CS8618

    public static StreakLog Create(int userId, DateOnly streakDate)
    {
        return new StreakLog
        {
            UserId = userId,
            StreakDate = streakDate
        };
    }
}
