using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StupifyConsoleApp.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Servers",
                table => new
                {
                    ServerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
                    DiscordGuildId = table.Column<long>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Servers", x => x.ServerId); });

            migrationBuilder.CreateTable(
                "Users",
                table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
                    DiscordUserId = table.Column<long>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Users", x => x.UserId); });

            migrationBuilder.CreateTable(
                "ServerUsers",
                table => new
                {
                    ServerUserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
                    ServerId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerUsers", x => x.ServerUserId);
                    table.ForeignKey(
                        "FK_ServerUsers_Servers_ServerId",
                        x => x.ServerId,
                        "Servers",
                        "ServerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_ServerUsers_Users_UserId",
                        x => x.UserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Quotes",
                table => new
                {
                    QuoteId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
                    QuoteBody = table.Column<string>(nullable: true),
                    ServerUserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.QuoteId);
                    table.ForeignKey(
                        "FK_Quotes_ServerUsers_ServerUserId",
                        x => x.ServerUserId,
                        "ServerUsers",
                        "ServerUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_Quotes_ServerUserId",
                "Quotes",
                "ServerUserId");

            migrationBuilder.CreateIndex(
                "IX_ServerUsers_ServerId",
                "ServerUsers",
                "ServerId");

            migrationBuilder.CreateIndex(
                "IX_ServerUsers_UserId",
                "ServerUsers",
                "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Quotes");

            migrationBuilder.DropTable(
                "ServerUsers");

            migrationBuilder.DropTable(
                "Servers");

            migrationBuilder.DropTable(
                "Users");
        }
    }
}