using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class ModifyDailyReadField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_mt_quiz_answer",
                table: "mt_quiz_answer");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "mt_quiz_answer",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "retry_seq",
                table: "mt_quiz_answer",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "minimal_correct_answer",
                table: "mt_daily_read",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_mt_quiz_answer",
                table: "mt_quiz_answer",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer",
                columns: new[] { "user_id", "daily_read_id", "question_seq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_mt_quiz_answer",
                table: "mt_quiz_answer");

            migrationBuilder.DropIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer");

            migrationBuilder.DropColumn(
                name: "id",
                table: "mt_quiz_answer");

            migrationBuilder.DropColumn(
                name: "retry_seq",
                table: "mt_quiz_answer");

            migrationBuilder.DropColumn(
                name: "minimal_correct_answer",
                table: "mt_daily_read");

            migrationBuilder.AddPrimaryKey(
                name: "pk_mt_quiz_answer",
                table: "mt_quiz_answer",
                columns: new[] { "user_id", "daily_read_id", "question_seq" });
        }
    }
}
