using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TelegramDownloadBot.Bot.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchResponse",
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
                    MagnetUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchResponse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CachedSearchResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SearchResponseId = table.Column<int>(type: "INTEGER", nullable: true),
                    OptionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedSearchResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CachedSearchResponses_SearchResponse_SearchResponseId",
                        column: x => x.SearchResponseId,
                        principalTable: "SearchResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CachedSearchResponses_SearchResponseId",
                table: "CachedSearchResponses",
                column: "SearchResponseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedSearchResponses");

            migrationBuilder.DropTable(
                name: "SearchResponse");
        }
    }
}
