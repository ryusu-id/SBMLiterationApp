using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class ReadingReportInsightLengthExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "insight",
                table: "mt_reading_report",
                type: "character varying(5000)",
                unicode: false,
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldUnicode: false,
                oldMaxLength: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "insight",
                table: "mt_reading_report",
                type: "character varying(1000)",
                unicode: false,
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldUnicode: false,
                oldMaxLength: 5000);
        }
    }
}
