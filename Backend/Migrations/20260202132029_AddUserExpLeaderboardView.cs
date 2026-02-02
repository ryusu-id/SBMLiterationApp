using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExpLeaderboardView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view for user exp leaderboard
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW user_exp_leaderboard AS
                WITH latest_snapshots AS (
                    SELECT DISTINCT ON (user_id)
                        user_id,
                        exp as snapshot_exp,
                        last_event_seq
                    FROM user_exp_snapshots
                    ORDER BY user_id, snapshot_seq DESC
                ),
                events_since_snapshots AS (
                    SELECT 
                        uee.user_id,
                        COALESCE(SUM(uee.exp), 0) as events_exp
                    FROM user_exp_events uee
                    LEFT JOIN latest_snapshots ls ON ls.user_id = uee.user_id
                    WHERE uee.id > COALESCE(ls.last_event_seq, 0)
                    GROUP BY uee.user_id
                ),
                all_users_with_exp AS (
                    SELECT DISTINCT user_id FROM user_exp_events
                )
                SELECT 
                    u.user_id,
                    COALESCE(ls.snapshot_exp, 0) + COALESCE(ess.events_exp, 0) as total_exp
                FROM all_users_with_exp u
                LEFT JOIN latest_snapshots ls ON ls.user_id = u.user_id
                LEFT JOIN events_since_snapshots ess ON ess.user_id = u.user_id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the view
            migrationBuilder.Sql("DROP VIEW IF EXISTS user_exp_leaderboard;");
        }
    }
}
