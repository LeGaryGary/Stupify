using Microsoft.EntityFrameworkCore.Migrations;

namespace Stupify.Data.Migrations
{
    public partial class someadjustmentscustomcommands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomCommands_Users_UserId",
                table: "CustomCommands");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CustomCommands",
                newName: "ServerId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomCommands_UserId",
                table: "CustomCommands",
                newName: "IX_CustomCommands_ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomCommands_Servers_ServerId",
                table: "CustomCommands",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomCommands_Servers_ServerId",
                table: "CustomCommands");

            migrationBuilder.RenameColumn(
                name: "ServerId",
                table: "CustomCommands",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomCommands_ServerId",
                table: "CustomCommands",
                newName: "IX_CustomCommands_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomCommands_Users_UserId",
                table: "CustomCommands",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
