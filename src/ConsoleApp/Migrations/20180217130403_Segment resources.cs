using Microsoft.EntityFrameworkCore.Migrations;

namespace StupifyConsoleApp.Migrations
{
    public partial class Segmentresources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "OutputPerTick",
                "Segments",
                "UnitsPerTick");

            migrationBuilder.AddColumn<decimal>(
                "Energy",
                "Segments",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                "EnergyPerTick",
                "Segments",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "Energy",
                "Segments");

            migrationBuilder.DropColumn(
                "EnergyPerTick",
                "Segments");

            migrationBuilder.RenameColumn(
                "UnitsPerTick",
                "Segments",
                "OutputPerTick");
        }
    }
}