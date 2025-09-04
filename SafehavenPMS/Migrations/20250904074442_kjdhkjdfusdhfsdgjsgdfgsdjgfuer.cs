using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class kjdhkjdfusdhfsdgjsgdfgsdjgfuer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diagnoses",
                columns: table => new
                {
                    DiagnosisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    IsDrugDependent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnoses", x => x.DiagnosisId);
                    table.ForeignKey(
                        name: "FK_Diagnoses_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalDiagnoses",
                columns: table => new
                {
                    MedicalDiagnosisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiagnosisId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalDiagnoses", x => x.MedicalDiagnosisId);
                    table.ForeignKey(
                        name: "FK_MedicalDiagnoses_Diagnoses_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalTable: "Diagnoses",
                        principalColumn: "DiagnosisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubstanceUses",
                columns: table => new
                {
                    SubstanceUseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiagnosisId = table.Column<int>(type: "int", nullable: false),
                    SubstanceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubstanceUses", x => x.SubstanceUseId);
                    table.ForeignKey(
                        name: "FK_SubstanceUses_Diagnoses_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalTable: "Diagnoses",
                        principalColumn: "DiagnosisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_InitialAssessmentFormId",
                table: "Diagnoses",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalDiagnoses_DiagnosisId",
                table: "MedicalDiagnoses",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_SubstanceUses_DiagnosisId",
                table: "SubstanceUses",
                column: "DiagnosisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalDiagnoses");

            migrationBuilder.DropTable(
                name: "SubstanceUses");

            migrationBuilder.DropTable(
                name: "Diagnoses");
        }
    }
}
