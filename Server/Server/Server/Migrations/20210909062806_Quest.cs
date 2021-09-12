using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class Quest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Player_OwnerPlayerDbId",
                table: "Quest");

            migrationBuilder.DropIndex(
                name: "IX_Quest_OwnerPlayerDbId",
                table: "Quest");

            migrationBuilder.DropColumn(
                name: "OwnerPlayerDbId",
                table: "Quest");

            migrationBuilder.AddColumn<int>(
                name: "OwnerDbId",
                table: "Quest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Quest_OwnerDbId",
                table: "Quest",
                column: "OwnerDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Player_OwnerDbId",
                table: "Quest",
                column: "OwnerDbId",
                principalTable: "Player",
                principalColumn: "PlayerDbId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Player_OwnerDbId",
                table: "Quest");

            migrationBuilder.DropIndex(
                name: "IX_Quest_OwnerDbId",
                table: "Quest");

            migrationBuilder.DropColumn(
                name: "OwnerDbId",
                table: "Quest");

            migrationBuilder.AddColumn<int>(
                name: "OwnerPlayerDbId",
                table: "Quest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quest_OwnerPlayerDbId",
                table: "Quest",
                column: "OwnerPlayerDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Player_OwnerPlayerDbId",
                table: "Quest",
                column: "OwnerPlayerDbId",
                principalTable: "Player",
                principalColumn: "PlayerDbId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
