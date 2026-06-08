using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintFromUserIdInVerificationTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VerificationTokens_UserId",
                table: "VerificationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationTokens_UserId",
                table: "VerificationTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VerificationTokens_UserId",
                table: "VerificationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationTokens_UserId",
                table: "VerificationTokens",
                column: "UserId",
                unique: true);
        }
    }
}
