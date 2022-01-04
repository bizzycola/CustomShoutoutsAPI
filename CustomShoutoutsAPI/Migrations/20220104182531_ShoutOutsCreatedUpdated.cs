using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomShoutoutsAPI.Migrations
{
    public partial class ShoutOutsCreatedUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "ShoutOuts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "ShoutOuts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "ShoutOuts");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "ShoutOuts");
        }
    }
}
