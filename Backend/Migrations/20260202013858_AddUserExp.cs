using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mt_streak_exp");

            migrationBuilder.DropIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer");

            migrationBuilder.DropPrimaryKey(
                name: "pk_mt_streak_log",
                table: "mt_streak_log");

            migrationBuilder.RenameTable(
                name: "mt_streak_log",
                newName: "streak_logs");

            migrationBuilder.RenameIndex(
                name: "ix_streak_log_user_date_unique",
                table: "streak_logs",
                newName: "IX_streak_logs_user_id_streak_date");

            migrationBuilder.AddColumn<long>(
                name: "CreateBy",
                table: "streak_logs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "CreateByStr",
                table: "streak_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "streak_logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "streak_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "UpdateBy",
                table: "streak_logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdateByStr",
                table: "streak_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "streak_logs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_streak_logs",
                table: "streak_logs",
                column: "id");

            migrationBuilder.CreateTable(
                name: "user_exp_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    event_seq = table.Column<int>(type: "integer", nullable: false),
                    exp = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    event_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ref_id = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdateBy = table.Column<long>(type: "bigint", nullable: true),
                    CreateByStr = table.Column<string>(type: "text", nullable: true),
                    UpdateByStr = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_exp_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_exp_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    snapshot_seq = table.Column<int>(type: "integer", nullable: false),
                    last_event_seq = table.Column<int>(type: "integer", nullable: false),
                    exp = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdateBy = table.Column<long>(type: "bigint", nullable: true),
                    CreateByStr = table.Column<string>(type: "text", nullable: true),
                    UpdateByStr = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_exp_snapshots", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer",
                columns: new[] { "user_id", "daily_read_id", "question_seq" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_exp_events_user_id_event_seq",
                table: "user_exp_events",
                columns: new[] { "user_id", "event_seq" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_exp_snapshots_user_id_snapshot_seq",
                table: "user_exp_snapshots",
                columns: new[] { "user_id", "snapshot_seq" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_exp_events");

            migrationBuilder.DropTable(
                name: "user_exp_snapshots");

            migrationBuilder.DropIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_streak_logs",
                table: "streak_logs");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "streak_logs");

            migrationBuilder.DropColumn(
                name: "CreateByStr",
                table: "streak_logs");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "streak_logs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "streak_logs");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                table: "streak_logs");

            migrationBuilder.DropColumn(
                name: "UpdateByStr",
                table: "streak_logs");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "streak_logs");

            migrationBuilder.RenameTable(
                name: "streak_logs",
                newName: "mt_streak_log");

            migrationBuilder.RenameIndex(
                name: "IX_streak_logs_user_id_streak_date",
                table: "mt_streak_log",
                newName: "ix_streak_log_user_date_unique");

            migrationBuilder.AddPrimaryKey(
                name: "pk_mt_streak_log",
                table: "mt_streak_log",
                column: "id");

            migrationBuilder.CreateTable(
                name: "mt_streak_exp",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    exp = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    streak_date_from = table.Column<DateOnly>(type: "date", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_streak_exp", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer",
                columns: new[] { "user_id", "daily_read_id", "question_seq" });

            migrationBuilder.CreateIndex(
                name: "ix_streak_exp_user_id",
                table: "mt_streak_exp",
                column: "user_id");
        }
    }
}
