using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace StupifyConsoleApp.Migrations
{
    public partial class stories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StoryInProgress",
                table: "Servers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ServerStories",
                columns: table => new
                {
                    ServerStoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EndTime = table.Column<DateTime>(nullable: false),
                    ServerId = table.Column<int>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    StoryInitiatedByServerUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerStories", x => x.ServerStoryId);
                    table.ForeignKey(
                        name: "FK_ServerStories_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerStories_ServerUsers_StoryInitiatedByServerUserId",
                        column: x => x.StoryInitiatedByServerUserId,
                        principalTable: "ServerUsers",
                        principalColumn: "ServerUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServerStoryParts",
                columns: table => new
                {
                    ServerStoryPartId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Part = table.Column<string>(nullable: true),
                    PartAuthorServerUserId = table.Column<int>(nullable: true),
                    PartNumber = table.Column<int>(nullable: false),
                    ServerStoryId = table.Column<int>(nullable: true),
                    TimeOfAddition = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerStoryParts", x => x.ServerStoryPartId);
                    table.ForeignKey(
                        name: "FK_ServerStoryParts_ServerUsers_PartAuthorServerUserId",
                        column: x => x.PartAuthorServerUserId,
                        principalTable: "ServerUsers",
                        principalColumn: "ServerUserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServerStoryParts_ServerStories_ServerStoryId",
                        column: x => x.ServerStoryId,
                        principalTable: "ServerStories",
                        principalColumn: "ServerStoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerStories_ServerId",
                table: "ServerStories",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerStories_StoryInitiatedByServerUserId",
                table: "ServerStories",
                column: "StoryInitiatedByServerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerStoryParts_PartAuthorServerUserId",
                table: "ServerStoryParts",
                column: "PartAuthorServerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerStoryParts_ServerStoryId",
                table: "ServerStoryParts",
                column: "ServerStoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerStoryParts");

            migrationBuilder.DropTable(
                name: "ServerStories");

            migrationBuilder.DropColumn(
                name: "StoryInProgress",
                table: "Servers");
        }
    }
}
