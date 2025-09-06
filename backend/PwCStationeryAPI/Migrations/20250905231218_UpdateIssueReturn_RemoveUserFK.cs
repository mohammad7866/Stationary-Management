using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PwCStationeryAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssueReturn_RemoveUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_AspNetUsers_IssuedByUserId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_AspNetUsers_ReturnedByUserId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_ReturnedByUserId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Issues_IssuedByUserId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "UnitPriceAtIssue",
                table: "IssueLines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceAtIssue",
                table: "IssueLines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ReturnedByUserId",
                table: "Returns",
                column: "ReturnedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_IssuedByUserId",
                table: "Issues",
                column: "IssuedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_AspNetUsers_IssuedByUserId",
                table: "Issues",
                column: "IssuedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_AspNetUsers_ReturnedByUserId",
                table: "Returns",
                column: "ReturnedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
