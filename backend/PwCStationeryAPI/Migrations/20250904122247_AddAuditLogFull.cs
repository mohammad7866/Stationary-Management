using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PwCStationeryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogFull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Entity_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_WhenUtc",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Entity",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 80);

            migrationBuilder.AddColumn<string>(
                name: "Controller",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMs",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Ip",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QueryString",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolesCsv",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RouteValuesJson",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimestampUtc",
                table: "AuditLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Method_StatusCode",
                table: "AuditLogs",
                columns: new[] { "Method", "StatusCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Path",
                table: "AuditLogs",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TimestampUtc",
                table: "AuditLogs",
                column: "TimestampUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Method_StatusCode",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Path",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TimestampUtc",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Controller",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Ip",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "QueryString",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RolesCsv",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RouteValuesJson",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TimestampUtc",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Entity",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entity_EntityId",
                table: "AuditLogs",
                columns: new[] { "Entity", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_WhenUtc",
                table: "AuditLogs",
                column: "WhenUtc");
        }
    }
}
