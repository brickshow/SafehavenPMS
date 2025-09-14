using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class sdsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PsyDiagnosisLists",
                columns: table => new
                {
                    PsyDiagnosisListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PsychiatricAssessmentId = table.Column<int>(type: "int", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsyDiagnosisLists", x => x.PsyDiagnosisListId);
                    table.ForeignKey(
                        name: "FK_PsyDiagnosisLists_PsychiatricAssessments_PsychiatricAssessmentId",
                        column: x => x.PsychiatricAssessmentId,
                        principalTable: "PsychiatricAssessments",
                        principalColumn: "PsychiatricAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PsyDiagnosisLists_PsychiatricAssessmentId",
                table: "PsyDiagnosisLists",
                column: "PsychiatricAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PsyDiagnosisLists");
        }
    }
}
