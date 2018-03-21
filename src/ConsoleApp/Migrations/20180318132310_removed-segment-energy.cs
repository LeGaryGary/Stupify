using Microsoft.EntityFrameworkCore.Migrations;

namespace StupifyConsoleApp.Migrations
{
    public partial class removedsegmentenergy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Energy",
                table: "Segments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Energy",
                table: "Segments",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
