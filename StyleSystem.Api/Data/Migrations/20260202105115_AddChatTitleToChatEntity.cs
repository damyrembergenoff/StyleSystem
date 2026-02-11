using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChatTitleToChatEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Title",
                table: "Chats",
                type: "smallint",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Chats");
        }
    }
}
