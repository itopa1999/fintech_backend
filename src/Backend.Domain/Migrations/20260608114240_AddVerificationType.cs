using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenType",
                table: "VerificationTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenType",
                table: "VerificationTokens");
        }
    }
}
