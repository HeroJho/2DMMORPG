using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class SkillDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Skill",
                columns: table => new
                {
                    SkillDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillLevelData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skill", x => x.SkillDbId);
                    table.ForeignKey(
                        name: "FK_Skill_Player_OwnerDbId",
                        column: x => x.OwnerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Skill_OwnerDbId",
                table: "Skill",
                column: "OwnerDbId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Skill");
        }
    }
}
