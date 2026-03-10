using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.Auth.Domain;
using PureTCOWebApp.Features.UserXpModule.Domain;

namespace PureTCOWebApp.Features.ActivityFeeds.Endpoints;

public record ActivityFeedDto(
    User User,
    string ActivityType,
    DateTime ActivityDate,
    string Description);

public class GetUserStreakEndpoint(ApplicationDbContext context)
    : Endpoint<PagingQuery, PagingResult<ActivityFeedDto>>
{
    public override void Configure()
    {
        Get("/feeds");
        Group<GlobalApiEndpointGroup>();
    }

    public override async Task HandleAsync(PagingQuery req, CancellationToken ct)
    {
        var query = from expEvent in context.UserExpEvents
                    join user in context.Users on expEvent.UserId equals user.Id
                    orderby expEvent.CreateTime descending
                    select new
                    {
                        User = user,
                        expEvent.EventName,
                        expEvent.CreateTime,
                        expEvent.Exp
                    };

        var result = await PagingService.PaginateQueryAsync(query, req, ct);

        var transformedResult = new PagingResult<ActivityFeedDto>(
            Rows: result.Rows.Select(item => new ActivityFeedDto(
                item.User,
                item.EventName,
                item.CreateTime,
                NaturalizeEventDescription(item.EventName, item.Exp)
            )),
            Page: result.Page,
            RowsPerPage: result.RowsPerPage,
            TotalRows: result.TotalRows,
            TotalPages: result.TotalPages,
            SearchText: result.SearchText,
            SortBy: result.SortBy,
            SortDirection: result.SortDirection
        );

        await Send.OkAsync(transformedResult, ct);
    }

    private string NaturalizeEventDescription(string eventName, decimal exp)
    {
        var roundedExp = (int)Math.Floor(exp);
        return eventName switch
        {
            "ReadingExp" => roundedExp > 0 ? $"Completed a reading session and earned {roundedExp} XP" : "Completed a reading session",
            "DailyReadsExp" => $"Successfully completed the daily read quiz and earned {roundedExp} XP",
            "StreakExp" => $"Achieved a {ExpConstants.STREAK_BONUS_DAYS}-day streak and earned bonus {roundedExp} XP",
            _ => $"Earned {roundedExp} XP from {eventName}"
        };
    }
}
