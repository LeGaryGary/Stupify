using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Stupify.Data.Migrations
{
    public partial class externalaccountslinking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalAuthentication",
                columns: table => new
                {
                    ExternalAuthenticationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccessTokenAes = table.Column<string>(nullable: true),
                    ExpiresInAes = table.Column<string>(nullable: true),
                    LastRefreshed = table.Column<DateTime>(nullable: false),
                    RefreshTokenAes = table.Column<string>(nullable: true),
                    ScopeAes = table.Column<string>(nullable: true),
                    Service = table.Column<int>(nullable: false),
                    TokenTypeAes = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalAuthentication", x => x.ExternalAuthenticationId);
                    table.ForeignKey(
                        name: "FK_ExternalAuthentication_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAuthentication_UserId",
                table: "ExternalAuthentication",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalAuthentication");
        }
    }
}
