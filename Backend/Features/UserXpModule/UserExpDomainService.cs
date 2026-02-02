using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.Auth.Domain;
using PureTCOWebApp.Features.UserXpModule.Domain;

namespace PureTCOWebApp.Features.UserXpModule;

public class UserExpDomainService(ApplicationDbContext _context)
{
    public int GetUserAccumulatedExp(int userId)
    {
        var sql = @"
            WITH latest_snapshot AS (
                SELECT 
                    COALESCE(exp, 0) as snapshot_exp,
                    COALESCE(last_event_seq, 0) as last_event_seq
                FROM user_exp_snapshots
                WHERE user_id = {0}
                ORDER BY snapshot_seq DESC
                LIMIT 1
            ),
            events_since_snapshot AS (
                SELECT COALESCE(SUM(exp), 0) as events_exp
                FROM user_exp_events
                WHERE user_id = {0}
                  AND id > COALESCE((SELECT last_event_seq FROM latest_snapshot), 0)
            )
            SELECT 
                CAST(COALESCE((SELECT snapshot_exp FROM latest_snapshot), 0) + 
                     COALESCE((SELECT events_exp FROM events_since_snapshot), 0) AS INTEGER) as ""Value""";

        var result = _context.Database
            .SqlQueryRaw<IntResult>(sql, userId)
            .AsEnumerable()
            .FirstOrDefault();

        return result?.Value ?? 0;
    }

    public async Task<int> GetNextEventSeq(int userId, CancellationToken ct = default)
    {
        var sql = "SELECT get_next_user_exp_event_seq({0}) as \"Value\"";
        
        var result = await _context.Database
            .SqlQueryRaw<IntResult>(sql, userId)
            .FirstOrDefaultAsync(ct);
        
        return result?.Value ?? 1;
    }

    public async Task<PagingResult<UserExp>> QueryLeaderboard(PagingQuery pagingQuery, CancellationToken ct = default)
    {
        // Override sort to always sort by exp descending in leaderboard
        pagingQuery = pagingQuery with { SortBy = "-TotalExp" };

        // Query the view which efficiently calculates exp for all users
        var query = from leaderboard in _context.UserExpLeaderboard
                    join user in _context.Users on leaderboard.UserId equals user.Id
                    select new
                    {
                        leaderboard.UserId,
                        Exp = leaderboard.TotalExp,
                        User = user
                    };

        // Use PagingService to paginate the query
        var result = await PagingService.PaginateQueryAsync(
            query,
            pagingQuery,
            item => new UserExp
            {
                Exp = item.Exp,
                User = item.User,
                Rank = 0 // Will be set after pagination
            },
            ct);

        // Calculate ranks based on the paginated results and page position
        var rankedItems = result.Rows.Select((item, index) =>
        {
            item.Rank = (pagingQuery.Page - 1) * pagingQuery.RowsPerPage + index + 1;
            return item;
        }).ToList();

        return new PagingResult<UserExp>(
            rankedItems,
            result.Page,
            result.RowsPerPage,
            result.TotalRows,
            result.TotalPages,
            result.SearchText,
            result.SortBy,
            result.SortDirection
        );
    }

    public class UserExp
    {
        public int Rank { get; set; }
        public decimal Exp { get; set; }
        public User User { get; set; } = null!;
    }
    
    private class IntResult
    {
        public int Value { get; set; }
    }
}