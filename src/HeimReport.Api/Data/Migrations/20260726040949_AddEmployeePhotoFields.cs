using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimReport.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePhotoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPublicId",
                table: "Employees",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Employees",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPublicId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Employees");
        }
    }
}
