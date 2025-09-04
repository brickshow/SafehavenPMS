using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class assessmentForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InitialAssessmentForms",
                columns: table => new
                {
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialAssessmentForms", x => x.InitialAssessmentFormId);
                    table.ForeignKey(
                        name: "FK_InitialAssessmentForms_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPresents",
                columns: table => new
                {
                    HistoryPresentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    OnsetOfDrugUse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonForFirstUse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HistoryOfImprisonment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousDrugRehab = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhoInvitedFirstUse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfPeopleFirstUse = table.Column<int>(type: "int", nullable: true),
                    LastUseOfSubstance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountConsumedFirstUse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPresents", x => x.HistoryPresentId);
                    table.ForeignKey(
                        name: "FK_HistoryPresents_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPresents_InitialAssessmentFormId",
                table: "HistoryPresents",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InitialAssessmentForms_PatientId",
                table: "InitialAssessmentForms",
                column: "PatientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryPresents");

            migrationBuilder.DropTable(
                name: "InitialAssessmentForms");
        }
    }
}
