using Microsoft.EntityFrameworkCore.Migrations;

namespace Stupify.Data.Migrations
{
    public partial class customcommandusesserveruser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomCommands_Servers_ServerId",
                table: "CustomCommands");

            migrationBuilder.RenameColumn(
                name: "ServerId",
                table: "CustomCommands",
                newName: "ServerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomCommands_ServerId",
                table: "CustomCommands",
                newName: "IX_CustomCommands_ServerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomCommands_ServerUsers_ServerUserId",
                table: "CustomCommands",
                column: "ServerUserId",
                principalTable: "ServerUsers",
                principalColumn: "ServerUserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomCommands_ServerUsers_ServerUserId",
                table: "CustomCommands");

            migrationBuilder.RenameColumn(
                name: "ServerUserId",
                table: "CustomCommands",
                newName: "ServerId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomCommands_ServerUserId",
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
    }
}
