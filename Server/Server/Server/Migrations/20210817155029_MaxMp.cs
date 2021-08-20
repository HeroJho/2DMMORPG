using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class MaxMp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxMp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxMp",
                table: "Player");
        }
    }
}
