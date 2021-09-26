using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class SkillPointsStatePointsJobClassType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobClassType",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkillPoints",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatPoints",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobClassType",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "SkillPoints",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatPoints",
                table: "Player");
        }
    }
}
