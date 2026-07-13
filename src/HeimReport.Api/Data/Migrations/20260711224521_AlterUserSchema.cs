using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimReport.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterUserSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailVerificationToken",
                table: "Users",
                newName: "EmailVerificationTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailVerificationTokenHash",
                table: "Users",
                newName: "EmailVerificationToken");
        }
    }
}
