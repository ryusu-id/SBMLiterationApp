using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class RemovedEventSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the function first since it depends on event_seq column
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_next_user_exp_event_seq(INTEGER);");

            migrationBuilder.DropIndex(
                name: "IX_user_exp_events_user_id_event_seq",
                table: "user_exp_events");

            migrationBuilder.DropColumn(
                name: "event_seq",
                table: "user_exp_events");

            migrationBuilder.CreateIndex(
                name: "IX_user_exp_events_user_id",
                table: "user_exp_events",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_exp_events_user_id",
                table: "user_exp_events");

            migrationBuilder.AddColumn<int>(
                name: "event_seq",
                table: "user_exp_events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_user_exp_events_user_id_event_seq",
                table: "user_exp_events",
                columns: new[] { "user_id", "event_seq" },
                unique: true);

            // Recreate the function when rolling back
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION get_next_user_exp_event_seq(p_user_id INTEGER)
                RETURNS INTEGER
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    next_seq INTEGER;
                BEGIN
                    SELECT COALESCE(MAX(event_seq), 0) + 1
                    INTO next_seq
                    FROM user_exp_events
                    WHERE user_id = p_user_id
                    FOR UPDATE;
                    
                    RETURN next_seq;
                END;
                $$;
            ");
        }
    }
}
