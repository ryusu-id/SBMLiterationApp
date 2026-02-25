using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PureTCOWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToGroupAndAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "uploaded_at",
                table: "assignment_submission_files",
                newName: "create_time");

            migrationBuilder.AddColumn<long>(
                name: "create_by",
                table: "groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "update_by",
                table: "groups",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "create_by",
                table: "group_members",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "create_time",
                table: "group_members",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "group_members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "update_by",
                table: "group_members",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_time",
                table: "group_members",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "create_by",
                table: "assignments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "update_by",
                table: "assignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "create_by",
                table: "assignment_submissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "assignment_submissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "update_by",
                table: "assignment_submissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_time",
                table: "assignment_submissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "create_by",
                table: "assignment_submission_files",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "assignment_submission_files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "update_by",
                table: "assignment_submission_files",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_time",
                table: "assignment_submission_files",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "create_by",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "status",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "update_by",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "create_by",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "create_time",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "status",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "update_by",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "update_time",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "create_by",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "status",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "update_by",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "create_by",
                table: "assignment_submissions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "assignment_submissions");

            migrationBuilder.DropColumn(
                name: "update_by",
                table: "assignment_submissions");

            migrationBuilder.DropColumn(
                name: "update_time",
                table: "assignment_submissions");

            migrationBuilder.DropColumn(
                name: "create_by",
                table: "assignment_submission_files");

            migrationBuilder.DropColumn(
                name: "status",
                table: "assignment_submission_files");

            migrationBuilder.DropColumn(
                name: "update_by",
                table: "assignment_submission_files");

            migrationBuilder.DropColumn(
                name: "update_time",
                table: "assignment_submission_files");

            migrationBuilder.RenameColumn(
                name: "create_time",
                table: "assignment_submission_files",
                newName: "uploaded_at");
        }
    }
}
