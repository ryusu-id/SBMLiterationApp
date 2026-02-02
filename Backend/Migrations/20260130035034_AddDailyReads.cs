using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyReads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mt_daily_read",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cover_img = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    exp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_daily_read", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mt_quiz_answer",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    daily_read_id = table.Column<int>(type: "integer", nullable: false),
                    question_seq = table.Column<int>(type: "integer", nullable: false),
                    answer = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_quiz_answer", x => new { x.user_id, x.daily_read_id, x.question_seq });
                });

            migrationBuilder.CreateTable(
                name: "mt_quiz_question",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    daily_read_id = table.Column<int>(type: "integer", nullable: false),
                    question_seq = table.Column<int>(type: "integer", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    correct_answer = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_quiz_question", x => x.id);
                    table.ForeignKey(
                        name: "FK_mt_quiz_question_mt_daily_read_daily_read_id",
                        column: x => x.daily_read_id,
                        principalTable: "mt_daily_read",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mt_quiz_choice",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quiz_question_id = table.Column<int>(type: "integer", nullable: false),
                    choice = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    answer = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_quiz_choice", x => x.id);
                    table.ForeignKey(
                        name: "FK_mt_quiz_choice_mt_quiz_question_quiz_question_id",
                        column: x => x.quiz_question_id,
                        principalTable: "mt_quiz_question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mt_quiz_choice_quiz_question_id",
                table: "mt_quiz_choice",
                column: "quiz_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_mt_quiz_question_daily_read_id",
                table: "mt_quiz_question",
                column: "daily_read_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mt_quiz_answer");

            migrationBuilder.DropTable(
                name: "mt_quiz_choice");

            migrationBuilder.DropTable(
                name: "mt_quiz_question");

            migrationBuilder.DropTable(
                name: "mt_daily_read");
        }
    }
}
