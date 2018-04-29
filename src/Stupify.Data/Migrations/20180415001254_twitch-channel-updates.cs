using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Stupify.Data.Migrations
{
    public partial class twitchchannelupdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TwitchUpdateChannel",
                table: "Servers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServerTwitchChannels",
                columns: table => new
                {
                    ServerTwitchChannelId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LastStatus = table.Column<bool>(nullable: false),
                    ServerId = table.Column<int>(nullable: true),
                    TwitchLoginName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerTwitchChannels", x => x.ServerTwitchChannelId);
                    table.ForeignKey(
                        name: "FK_ServerTwitchChannels_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerTwitchChannels_ServerId",
                table: "ServerTwitchChannels",
                column: "ServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerTwitchChannels");

            migrationBuilder.DropColumn(
                name: "TwitchUpdateChannel",
                table: "Servers");
        }
    }
}
