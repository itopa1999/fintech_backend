using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIntToStringForToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "VerificationTokens",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Token",
                table: "VerificationTokens",
                type: "integer",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6);
        }
    }
}
