using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolTrackingSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ToolIssuances",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ToolIssuances_ApplicationUserId",
                table: "ToolIssuances",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToolIssuances_AspNetUsers_ApplicationUserId",
                table: "ToolIssuances",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToolIssuances_AspNetUsers_ApplicationUserId",
                table: "ToolIssuances");

            migrationBuilder.DropIndex(
                name: "IX_ToolIssuances_ApplicationUserId",
                table: "ToolIssuances");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ToolIssuances");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");
        }
    }
}
