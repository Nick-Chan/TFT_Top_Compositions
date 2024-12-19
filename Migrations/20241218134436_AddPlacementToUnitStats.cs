using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TFT.Migrations
{
    /// <inheritdoc />
    public partial class AddPlacementToUnitStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Placement",
                table: "UnitStats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Placement",
                table: "UnitStats");
        }
    }
}
