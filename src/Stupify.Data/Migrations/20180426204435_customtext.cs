using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Stupify.Data.Migrations
{
    public partial class customtext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BanChannel",
                table: "ServerSettings",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "KickChannel",
                table: "ServerSettings",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LeaveChannel",
                table: "ServerSettings",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WelcomeChannel",
                table: "ServerSettings",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServerCustomTexts",
                columns: table => new
                {
                    ServerCustomTextId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ServerId = table.Column<int>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerCustomTexts", x => x.ServerCustomTextId);
                    table.ForeignKey(
                        name: "FK_ServerCustomTexts_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerCustomTexts_ServerId",
                table: "ServerCustomTexts",
                column: "ServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerCustomTexts");

            migrationBuilder.DropColumn(
                name: "BanChannel",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "KickChannel",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "LeaveChannel",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "WelcomeChannel",
                table: "ServerSettings");
        }
    }
}
