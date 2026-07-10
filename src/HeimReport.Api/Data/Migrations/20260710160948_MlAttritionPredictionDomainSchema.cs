using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HeimReport.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class MlAttritionPredictionDomainSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MlModelVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Algorithm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TrainedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metrics = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MlModelVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttritionPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    MlModelVersionId = table.Column<int>(type: "integer", nullable: false),
                    PredictionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RiskScore = table.Column<float>(type: "real", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttritionPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttritionPredictions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttritionPredictions_MlModelVersions_MlModelVersionId",
                        column: x => x.MlModelVersionId,
                        principalTable: "MlModelVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttritionFactors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttritionPredictionId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContributionScore = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttritionFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttritionFactors_AttritionPredictions_AttritionPredictionId",
                        column: x => x.AttritionPredictionId,
                        principalTable: "AttritionPredictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AttritionPredictionId = table.Column<int>(type: "integer", nullable: false),
                    SuggestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_AttritionPredictions_AttritionPredictionId",
                        column: x => x.AttritionPredictionId,
                        principalTable: "AttritionPredictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttritionFactors_AttritionPredictionId",
                table: "AttritionFactors",
                column: "AttritionPredictionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttritionPredictions_EmployeeId_PredictionDate",
                table: "AttritionPredictions",
                columns: new[] { "EmployeeId", "PredictionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AttritionPredictions_MlModelVersionId",
                table: "AttritionPredictions",
                column: "MlModelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_AttritionPredictionId",
                table: "Recommendations",
                column: "AttritionPredictionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttritionFactors");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "AttritionPredictions");

            migrationBuilder.DropTable(
                name: "MlModelVersions");
        }
    }
}
