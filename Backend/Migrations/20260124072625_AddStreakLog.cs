using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mt_streak_exp_mt_reading_resource_reading_resource_id",
                table: "mt_streak_exp");

            migrationBuilder.DropIndex(
                name: "IX_mt_streak_exp_reading_resource_id",
                table: "mt_streak_exp");

            migrationBuilder.DropColumn(
                name: "reading_resource_id",
                table: "mt_streak_exp");

            migrationBuilder.CreateTable(
                name: "mt_streak_log",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    streak_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_streak_log", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_streak_log_user_date_unique",
                table: "mt_streak_log",
                columns: new[] { "user_id", "streak_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mt_streak_log");

            migrationBuilder.AddColumn<int>(
                name: "reading_resource_id",
                table: "mt_streak_exp",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_mt_streak_exp_reading_resource_id",
                table: "mt_streak_exp",
                column: "reading_resource_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mt_streak_exp_mt_reading_resource_reading_resource_id",
                table: "mt_streak_exp",
                column: "reading_resource_id",
                principalTable: "mt_reading_resource",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
