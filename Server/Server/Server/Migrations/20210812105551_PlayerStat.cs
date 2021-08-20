using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class PlayerStat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Player_Account_AccountDbId",
                table: "Player");

            migrationBuilder.AlterColumn<int>(
                name: "AccountDbId",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Attack",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxHp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Speed",
                table: "Player",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "TotalExp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Account_AccountDbId",
                table: "Player",
                column: "AccountDbId",
                principalTable: "Account",
                principalColumn: "AccountDbId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Player_Account_AccountDbId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Attack",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Hp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MaxHp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "TotalExp",
                table: "Player");

            migrationBuilder.AlterColumn<int>(
                name: "AccountDbId",
                table: "Player",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Account_AccountDbId",
                table: "Player",
                column: "AccountDbId",
                principalTable: "Account",
                principalColumn: "AccountDbId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
