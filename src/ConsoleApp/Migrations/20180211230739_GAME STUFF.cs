using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StupifyConsoleApp.Migrations
{
    public partial class GAMESTUFF : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                "Balance",
                "Users",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                "Segments",
                table => new
                {
                    SegmentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
                    OutputPerTick = table.Column<decimal>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Segments", x => x.SegmentId); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Segments");

            migrationBuilder.DropColumn(
                "Balance",
                "Users");
        }
    }
}