using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TelegramDownloadBot.Bot.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Uploaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Size = table.Column<uint>(type: "INTEGER", nullable: false),
                    NumOfFiles = table.Column<uint>(type: "INTEGER", nullable: false),
                    Seeders = table.Column<uint>(type: "INTEGER", nullable: false),
                    Peers = table.Column<uint>(type: "INTEGER", nullable: false),
                    MagnetUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Guid = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResponses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchResponses");
        }
    }
}
