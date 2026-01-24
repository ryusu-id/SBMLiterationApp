using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveISBNIndexFromReadingResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_mt_reading_resource_isbn",
                table: "mt_reading_resource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_mt_reading_resource_isbn",
                table: "mt_reading_resource",
                column: "isbn");
        }
    }
}
