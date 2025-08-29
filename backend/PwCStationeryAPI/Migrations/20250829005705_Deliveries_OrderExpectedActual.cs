using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PwCStationeryAPI.Migrations
{
    /// <inheritdoc />
    public partial class Deliveries_OrderExpectedActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Offices_Name",
                table: "Offices");

            migrationBuilder.DropIndex(
                name: "IX_Items_Name_SupplierId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_Office_ScheduledDateUtc",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_SupplierId_ScheduledDateUtc",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "ScheduledDateUtc",
                table: "Deliveries",
                newName: "OrderedDateUtc");

            migrationBuilder.RenameColumn(
                name: "ArrivalDateUtc",
                table: "Deliveries",
                newName: "ExpectedArrivalDateUtc");

            migrationBuilder.AlterColumn<int>(
                name: "ReorderThreshold",
                table: "StockLevels",
                type: "INTEGER",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualArrivalDateUtc",
                table: "Deliveries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offices_Name",
                table: "Offices",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Name",
                table: "Items",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_Office_Status",
                table: "Deliveries",
                columns: new[] { "Office", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderedDateUtc",
                table: "Deliveries",
                column: "OrderedDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_SupplierId",
                table: "Deliveries",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Offices_Name",
                table: "Offices");

            migrationBuilder.DropIndex(
                name: "IX_Items_Name",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_Office_Status",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_OrderedDateUtc",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_SupplierId",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ActualArrivalDateUtc",
                table: "Deliveries");

            migrationBuilder.RenameColumn(
                name: "OrderedDateUtc",
                table: "Deliveries",
                newName: "ScheduledDateUtc");

            migrationBuilder.RenameColumn(
                name: "ExpectedArrivalDateUtc",
                table: "Deliveries",
                newName: "ArrivalDateUtc");

            migrationBuilder.AlterColumn<int>(
                name: "ReorderThreshold",
                table: "StockLevels",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Offices_Name",
                table: "Offices",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Name_SupplierId",
                table: "Items",
                columns: new[] { "Name", "SupplierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_Office_ScheduledDateUtc",
                table: "Deliveries",
                columns: new[] { "Office", "ScheduledDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_SupplierId_ScheduledDateUtc",
                table: "Deliveries",
                columns: new[] { "SupplierId", "ScheduledDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }
    }
}
