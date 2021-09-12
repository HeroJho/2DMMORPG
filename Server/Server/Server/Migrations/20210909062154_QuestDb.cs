using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class QuestDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quest",
                columns: table => new
                {
                    QuestDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TmeplateId = table.Column<int>(type: "int", nullable: false),
                    QuestState = table.Column<int>(type: "int", nullable: false),
                    CurrentNumber = table.Column<int>(type: "int", nullable: true),
                    OwnerPlayerDbId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quest", x => x.QuestDbId);
                    table.ForeignKey(
                        name: "FK_Quest_Player_OwnerPlayerDbId",
                        column: x => x.OwnerPlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quest_OwnerPlayerDbId",
                table: "Quest",
                column: "OwnerPlayerDbId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quest");
        }
    }
}
