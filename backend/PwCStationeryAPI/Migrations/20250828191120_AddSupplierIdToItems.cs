using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PwCStationeryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierIdToItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_StockLevels_ItemId",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "Deliveries");

            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "Requests",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "Supplier",
                table: "Deliveries",
                newName: "ScheduledDateUtc");

            migrationBuilder.RenameColumn(
                name: "ArrivalDate",
                table: "Deliveries",
                newName: "ArrivalDateUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Suppliers",
                type: "TEXT",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Suppliers",
                type: "TEXT",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ReorderThreshold",
                table: "StockLevels",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Requests",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionAtUtc",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Requests",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Deliveries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StockLevels_ItemId_OfficeId",
                table: "StockLevels",
                columns: new[] { "ItemId", "OfficeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Office_Status",
                table: "Requests",
                columns: new[] { "Office", "Status" });

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
                name: "IX_Items_SupplierId",
                table: "Items",
                column: "SupplierId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId",
                table: "Deliveries",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Suppliers_SupplierId",
                table: "Items",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Suppliers_SupplierId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_StockLevels_ItemId_OfficeId",
                table: "StockLevels");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Office_Status",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Offices_Name",
                table: "Offices");

            migrationBuilder.DropIndex(
                name: "IX_Items_Name_SupplierId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_SupplierId",
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

            migrationBuilder.DropColumn(
                name: "ReorderThreshold",
                table: "StockLevels");

            migrationBuilder.DropColumn(
                name: "DecisionAtUtc",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Deliveries");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Requests",
                newName: "RequestDate");

            migrationBuilder.RenameColumn(
                name: "ScheduledDateUtc",
                table: "Deliveries",
                newName: "Supplier");

            migrationBuilder.RenameColumn(
                name: "ArrivalDateUtc",
                table: "Deliveries",
                newName: "ArrivalDate");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactEmail",
                table: "Suppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Requests",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40,
                oldDefaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "Deliveries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_StockLevels_ItemId",
                table: "StockLevels",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
