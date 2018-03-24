using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Stupify.Data.Migrations
{
    public partial class stories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                "StoryInProgress",
                "Servers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                "ServerStories",
                table => new
                {
                    ServerStoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
                    EndTime = table.Column<DateTime>(nullable: false),
                    ServerId = table.Column<int>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    StoryInitiatedByServerUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerStories", x => x.ServerStoryId);
                    table.ForeignKey(
                        "FK_ServerStories_Servers_ServerId",
                        x => x.ServerId,
                        "Servers",
                        "ServerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_ServerStories_ServerUsers_StoryInitiatedByServerUserId",
                        x => x.StoryInitiatedByServerUserId,
                        "ServerUsers",
                        "ServerUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "ServerStoryParts",
                table => new
                {
                    ServerStoryPartId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
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
                        "FK_ServerStoryParts_ServerUsers_PartAuthorServerUserId",
                        x => x.PartAuthorServerUserId,
                        "ServerUsers",
                        "ServerUserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_ServerStoryParts_ServerStories_ServerStoryId",
                        x => x.ServerStoryId,
                        "ServerStories",
                        "ServerStoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_ServerStories_ServerId",
                "ServerStories",
                "ServerId");

            migrationBuilder.CreateIndex(
                "IX_ServerStories_StoryInitiatedByServerUserId",
                "ServerStories",
                "StoryInitiatedByServerUserId");

            migrationBuilder.CreateIndex(
                "IX_ServerStoryParts_PartAuthorServerUserId",
                "ServerStoryParts",
                "PartAuthorServerUserId");

            migrationBuilder.CreateIndex(
                "IX_ServerStoryParts_ServerStoryId",
                "ServerStoryParts",
                "ServerStoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "ServerStoryParts");

            migrationBuilder.DropTable(
                "ServerStories");

            migrationBuilder.DropColumn(
                "StoryInProgress",
                "Servers");
        }
    }
}