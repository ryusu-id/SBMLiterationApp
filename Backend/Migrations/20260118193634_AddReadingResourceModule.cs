using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingResourceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mt_reading_resource",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    isbn = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    book_category = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    authors = table.Column<string>(type: "character varying(300)", unicode: false, maxLength: 300, nullable: false),
                    publish_year = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    page = table.Column<int>(type: "integer", nullable: false),
                    resource_link = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    resource_type = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "BOOK"),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_reading_resource", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mt_reading_report",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    reading_resource_id = table.Column<int>(type: "integer", nullable: false),
                    report_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    current_page = table.Column<int>(type: "integer", nullable: false),
                    insight = table.Column<string>(type: "character varying(1000)", unicode: false, maxLength: 1000, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now())"),
                    update_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<long>(type: "bigint", nullable: false),
                    update_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mt_reading_report", x => x.id);
                    table.ForeignKey(
                        name: "FK_mt_reading_report_mt_reading_resource_reading_resource_id",
                        column: x => x.reading_resource_id,
                        principalTable: "mt_reading_resource",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reading_report_resource_id",
                table: "mt_reading_report",
                column: "reading_resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_reading_report_user_id",
                table: "mt_reading_report",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_mt_reading_resource_isbn",
                table: "mt_reading_resource",
                column: "isbn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mt_reading_report");

            migrationBuilder.DropTable(
                name: "mt_reading_resource");
        }
    }
}
