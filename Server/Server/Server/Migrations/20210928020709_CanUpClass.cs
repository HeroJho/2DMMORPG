using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class CanUpClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanUpClass",
                table: "Player",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanUpClass",
                table: "Player");
        }
    }
}
