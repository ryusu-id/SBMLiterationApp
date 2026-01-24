using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNewReadingModuleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "book_category",
                table: "mt_reading_resource",
                newName: "reading_category");

            migrationBuilder.AddColumn<string>(
                name: "css_class",
                table: "mt_reading_resource",
                type: "character varying(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "mt_reading_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_name = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_reading_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mt_reading_recommendation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    isbn = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    reading_category = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    authors = table.Column<string>(type: "character varying(300)", unicode: false, maxLength: 300, nullable: false),
                    publish_year = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    page = table.Column<int>(type: "integer", nullable: false),
                    resource_link = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_reading_recommendation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mt_streak_exp",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    streak_date_from = table.Column<DateOnly>(type: "date", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    exp = table.Column<int>(type: "integer", nullable: false),
                    reading_resource_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_streak_exp", x => x.id);
                    table.ForeignKey(
                        name: "FK_mt_streak_exp_mt_reading_resource_reading_resource_id",
                        column: x => x.reading_resource_id,
                        principalTable: "mt_reading_resource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reading_recommendation_isbn",
                table: "mt_reading_recommendation",
                column: "isbn");

            migrationBuilder.CreateIndex(
                name: "IX_mt_streak_exp_reading_resource_id",
                table: "mt_streak_exp",
                column: "reading_resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_streak_exp_user_id",
                table: "mt_streak_exp",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mt_reading_category");

            migrationBuilder.DropTable(
                name: "mt_reading_recommendation");

            migrationBuilder.DropTable(
                name: "mt_streak_exp");

            migrationBuilder.DropColumn(
                name: "css_class",
                table: "mt_reading_resource");

            migrationBuilder.RenameColumn(
                name: "reading_category",
                table: "mt_reading_resource",
                newName: "book_category");
        }
    }
}
