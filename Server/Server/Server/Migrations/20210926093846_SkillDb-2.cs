using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class SkillDb2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillPoints",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "SkillPoints",
                table: "Skill",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillPoints",
                table: "Skill");

            migrationBuilder.AddColumn<int>(
                name: "SkillPoints",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
