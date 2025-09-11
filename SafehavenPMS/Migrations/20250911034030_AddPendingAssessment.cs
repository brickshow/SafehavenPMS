using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChiefComplaint",
                table: "PsychiatricAssessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HistoryOfPresentIllness",
                table: "PsychiatricAssessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Impression",
                table: "PsychiatricAssessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MentalStatusExamination",
                table: "PsychiatricAssessments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalAndFamilyHistory",
                table: "PsychiatricAssessments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChiefComplaint",
                table: "PsychiatricAssessments");

            migrationBuilder.DropColumn(
                name: "HistoryOfPresentIllness",
                table: "PsychiatricAssessments");

            migrationBuilder.DropColumn(
                name: "Impression",
                table: "PsychiatricAssessments");

            migrationBuilder.DropColumn(
                name: "MentalStatusExamination",
                table: "PsychiatricAssessments");

            migrationBuilder.DropColumn(
                name: "PersonalAndFamilyHistory",
                table: "PsychiatricAssessments");
        }
    }
}
