using Microsoft.EntityFrameworkCore.Migrations;

namespace TelegramDownloadBot.Bot.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CachedSearchResponses_SearchResponse_SearchResponseId",
                table: "CachedSearchResponses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchResponse",
                table: "SearchResponse");

            migrationBuilder.RenameTable(
                name: "SearchResponse",
                newName: "SearchResponses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchResponses",
                table: "SearchResponses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CachedSearchResponses_SearchResponses_SearchResponseId",
                table: "CachedSearchResponses",
                column: "SearchResponseId",
                principalTable: "SearchResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CachedSearchResponses_SearchResponses_SearchResponseId",
                table: "CachedSearchResponses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchResponses",
                table: "SearchResponses");

            migrationBuilder.RenameTable(
                name: "SearchResponses",
                newName: "SearchResponse");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchResponse",
                table: "SearchResponse",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CachedSearchResponses_SearchResponse_SearchResponseId",
                table: "CachedSearchResponses",
                column: "SearchResponseId",
                principalTable: "SearchResponse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
