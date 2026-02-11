using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recommendations_Gender_SkinTone",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "FemaleBodyType",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "MaleBodyType",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "SkinTone",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Recommendations");

            migrationBuilder.AddColumn<string>(
                name: "FemaleBodyType",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaleBodyType",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkinTone",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "Users",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FemaleBodyType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MaleBodyType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SkinTone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "FemaleBodyType",
                table: "Recommendations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Recommendations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Recommendations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaleBodyType",
                table: "Recommendations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkinTone",
                table: "Recommendations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "Recommendations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_Gender_SkinTone",
                table: "Recommendations",
                columns: new[] { "Gender", "SkinTone" });
        }
    }
}
