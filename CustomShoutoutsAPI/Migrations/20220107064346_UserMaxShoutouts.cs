using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomShoutoutsAPI.Migrations
{
    public partial class UserMaxShoutouts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAllowedShoutouts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAllowedShoutouts",
                table: "Users");
        }
    }
}
