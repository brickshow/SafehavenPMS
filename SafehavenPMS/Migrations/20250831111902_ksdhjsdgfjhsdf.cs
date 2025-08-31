using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class ksdhjsdgfjhsdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounselorImpressions");

            migrationBuilder.DropTable(
                name: "PresentingProblems");

            migrationBuilder.RenameColumn(
                name: "OtherFamilyDetails",
                table: "IntakeForms",
                newName: "ProblemPresentation");

            migrationBuilder.AddColumn<string>(
                name: "CouncilorImpression",
                table: "IntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherFamilyDetails",
                table: "FamilyMembers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouncilorImpression",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "OtherFamilyDetails",
                table: "FamilyMembers");

            migrationBuilder.RenameColumn(
                name: "ProblemPresentation",
                table: "IntakeForms",
                newName: "OtherFamilyDetails");

            migrationBuilder.CreateTable(
                name: "CounselorImpressions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientIntakeIntakeFormsId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntakeFormId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounselorImpressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounselorImpressions_IntakeForms_IntakeFormId",
                        column: x => x.IntakeFormId,
                        principalTable: "IntakeForms",
                        principalColumn: "IntakeFormsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounselorImpressions_IntakeForms_PatientIntakeIntakeFormsId",
                        column: x => x.PatientIntakeIntakeFormsId,
                        principalTable: "IntakeForms",
                        principalColumn: "IntakeFormsId");
                });

            migrationBuilder.CreateTable(
                name: "PresentingProblems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientIntakeIntakeFormsId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntakeFormId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresentingProblems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresentingProblems_IntakeForms_IntakeFormId",
                        column: x => x.IntakeFormId,
                        principalTable: "IntakeForms",
                        principalColumn: "IntakeFormsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresentingProblems_IntakeForms_PatientIntakeIntakeFormsId",
                        column: x => x.PatientIntakeIntakeFormsId,
                        principalTable: "IntakeForms",
                        principalColumn: "IntakeFormsId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounselorImpressions_IntakeFormId",
                table: "CounselorImpressions",
                column: "IntakeFormId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselorImpressions_PatientIntakeIntakeFormsId",
                table: "CounselorImpressions",
                column: "PatientIntakeIntakeFormsId");

            migrationBuilder.CreateIndex(
                name: "IX_PresentingProblems_IntakeFormId",
                table: "PresentingProblems",
                column: "IntakeFormId");

            migrationBuilder.CreateIndex(
                name: "IX_PresentingProblems_PatientIntakeIntakeFormsId",
                table: "PresentingProblems",
                column: "PatientIntakeIntakeFormsId");
        }
    }
}
