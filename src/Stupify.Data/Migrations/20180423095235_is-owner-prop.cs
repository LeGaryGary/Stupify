using Microsoft.EntityFrameworkCore.Migrations;

namespace Stupify.Data.Migrations
{
    public partial class isownerprop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOwner",
                table: "ServerUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOwner",
                table: "ServerUsers");
        }
    }
}
