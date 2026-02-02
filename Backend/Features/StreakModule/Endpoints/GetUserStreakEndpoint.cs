using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.UserXpModule;

namespace PureTCOWebApp.Features.StreakModule.Endpoints;

public record DayStreakStatus(
    string Date,
    bool? HasStreak);

public record GetUserStreakResponse(
    int CurrentStreakDays,
    int TotalExp,
    List<DayStreakStatus> WeeklyStatus);

public class GetUserStreakEndpoint(ApplicationDbContext context, UserExpDomainService userExpService)
    : EndpointWithoutRequest<ApiResponse<GetUserStreakResponse>>
{
    public override void Configure()
    {
        Get("/me");
        Group<StreakEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(8));

        // Get weekly status (Monday to Sunday)
        var daysSinceMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
        var startOfWeek = today.AddDays(-daysSinceMonday);

        var streakLogs = await context.StreakLogs
            .AsNoTracking()
            .Where(s => s.UserId == userId && 
                       s.StreakDate >= startOfWeek && 
                       s.StreakDate <= startOfWeek.AddDays(6))
            .Select(s => s.StreakDate)
            .ToListAsync(ct);

        var weeklyStatus = new List<DayStreakStatus>();
        
        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            bool? hasStreak = date > today ? null : streakLogs.Contains(date);
            weeklyStatus.Add(new DayStreakStatus(date.ToString("yyyy-MM-dd"), hasStreak));
        }

        // Calculate current consecutive streak
        var allStreakLogs = await context.StreakLogs
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StreakDate)
            .Select(s => s.StreakDate)
            .ToListAsync(ct);

        int currentStreak = 0;
        if (allStreakLogs.FirstOrDefault() == today)
            currentStreak = 1; // Count today

        // check from today backwards
        DateOnly expectedDate = today.AddDays(-1);
        foreach (var logDate in allStreakLogs.Where(e => e < today))
        {
            if (logDate == expectedDate)
            {
                currentStreak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else if (logDate < expectedDate)
            {
                break; // Streak is broken
            }
        }

        // Get total accumulated exp from UserExpDomainService
        var totalExp = userExpService.GetUserAccumulatedExp(userId);

        var response = new GetUserStreakResponse(
            CurrentStreakDays: currentStreak,
            TotalExp: totalExp,
            WeeklyStatus: weeklyStatus);

        await Send.OkAsync(Result.Success(response), ct);
    }
}
