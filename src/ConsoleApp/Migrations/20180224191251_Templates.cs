using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StupifyConsoleApp.Migrations
{
    public partial class Templates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Segments",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "SegmentTemplates",
                columns: table => new
                {
                    SegmentTemplateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentTemplates", x => x.SegmentTemplateId);
                    table.ForeignKey(
                        name: "FK_SegmentTemplates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Segments_UserId",
                table: "Segments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTemplates_UserId",
                table: "SegmentTemplates",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Users_UserId",
                table: "Segments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Users_UserId",
                table: "Segments");

            migrationBuilder.DropTable(
                name: "SegmentTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Segments_UserId",
                table: "Segments");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Segments",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
