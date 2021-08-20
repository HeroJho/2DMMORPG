using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class Mp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Mp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mp",
                table: "Player");
        }
    }
}
