using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class ModifyQuizAnswerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer");

            migrationBuilder.CreateIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer",
                columns: new[] { "user_id", "daily_read_id", "question_seq", "retry_seq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer");

            migrationBuilder.CreateIndex(
                name: "ix_mt_quiz_answer_user_daily_question",
                table: "mt_quiz_answer",
                columns: new[] { "user_id", "daily_read_id", "question_seq" });
        }
    }
}
