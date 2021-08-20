using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class Equipped : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Equipped",
                table: "Item",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Equipped",
                table: "Item");
        }
    }
}
