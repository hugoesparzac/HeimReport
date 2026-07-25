using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeimReport.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeJobHistoryChangeReasonToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeReason",
                table: "EmployeeJobHistories");

            migrationBuilder.AddColumn<int>(
                name: "ChangeReason",
                table: "EmployeeJobHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherReasonDetail",
                table: "EmployeeJobHistories",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherReasonDetail",
                table: "EmployeeJobHistories");

            migrationBuilder.DropColumn(
                name: "ChangeReason",
                table: "EmployeeJobHistories");

            migrationBuilder.AddColumn<string>(
                name: "ChangeReason",
                table: "EmployeeJobHistories",
                type: "text",
                nullable: true);
        }
    }
}