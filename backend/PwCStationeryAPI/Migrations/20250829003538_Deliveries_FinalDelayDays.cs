using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PwCStationeryAPI.Migrations
{
    /// <inheritdoc />
    public partial class Deliveries_FinalDelayDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinalDelayDays",
                table: "Deliveries",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalDelayDays",
                table: "Deliveries");
        }
    }
}
