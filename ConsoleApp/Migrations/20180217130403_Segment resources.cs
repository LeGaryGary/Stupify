using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace StupifyConsoleApp.Migrations
{
    public partial class Segmentresources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutputPerTick",
                table: "Segments",
                newName: "UnitsPerTick");

            migrationBuilder.AddColumn<decimal>(
                name: "Energy",
                table: "Segments",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EnergyPerTick",
                table: "Segments",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Energy",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "EnergyPerTick",
                table: "Segments");

            migrationBuilder.RenameColumn(
                name: "UnitsPerTick",
                table: "Segments",
                newName: "OutputPerTick");
        }
    }
}
