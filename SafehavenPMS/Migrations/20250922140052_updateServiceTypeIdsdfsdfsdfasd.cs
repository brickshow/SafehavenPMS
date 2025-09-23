using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class updateServiceTypeIdsdfsdfsdfasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressNotes",
                columns: table => new
                {
                    ProgressNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    InterventionId = table.Column<int>(type: "int", nullable: true),
                    Clinician = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoapRaw = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Subjective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Assessment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Plan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressNotes", x => x.ProgressNoteId);
                    table.ForeignKey(
                        name: "FK_ProgressNotes_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "InterventionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressNotes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressNotes_InterventionId",
                table: "ProgressNotes",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressNotes_PatientId",
                table: "ProgressNotes",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressNotes");
        }
    }
}
