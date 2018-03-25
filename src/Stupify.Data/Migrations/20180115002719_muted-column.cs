using Microsoft.EntityFrameworkCore.Migrations;

namespace Stupify.Data.Migrations
{
    public partial class mutedcolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                "Muted",
                "ServerUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "Muted",
                "ServerUsers");
        }
    }
}