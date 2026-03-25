using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteChatEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Recommendations_CreatedAt",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Recommendations");

            migrationBuilder.RenameColumn(
                name: "TextRecommendation",
                table: "Recommendations",
                newName: "Temperature");

            migrationBuilder.RenameColumn(
                name: "ImagePrompt",
                table: "Recommendations",
                newName: "Season");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Recommendations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalPreferences",
                table: "Recommendations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occasion",
                table: "Recommendations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendationText",
                table: "Recommendations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecommendationImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    RecommendationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationImages_Recommendations_RecommendationId",
                        column: x => x.RecommendationId,
                        principalTable: "Recommendations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationImages_CreatedAt",
                table: "RecommendationImages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationImages_RecommendationId",
                table: "RecommendationImages",
                column: "RecommendationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecommendationImages");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AdditionalPreferences",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "Occasion",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "RecommendationText",
                table: "Recommendations");

            migrationBuilder.RenameColumn(
                name: "Temperature",
                table: "Recommendations",
                newName: "TextRecommendation");

            migrationBuilder.RenameColumn(
                name: "Season",
                table: "Recommendations",
                newName: "ImagePrompt");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Recommendations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Recommendations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<short>(type: "smallint", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_CreatedAt",
                table: "Recommendations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserId",
                table: "Chats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                column: "ChatId");
        }
    }
}
