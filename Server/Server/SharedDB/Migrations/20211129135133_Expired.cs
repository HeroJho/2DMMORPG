using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SharedDB.Migrations
{
    public partial class Expired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerInfo",
                columns: table => new
                {
                    ServerDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Port = table.Column<int>(type: "int", nullable: false),
                    BusyScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerInfo", x => x.ServerDbId);
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    TokenDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountDbId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<int>(type: "int", nullable: false),
                    Expired = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.TokenDbId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerInfo_Name",
                table: "ServerInfo",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Token_AccountDbId",
                table: "Token",
                column: "AccountDbId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerInfo");

            migrationBuilder.DropTable(
                name: "Token");
        }
    }
}
