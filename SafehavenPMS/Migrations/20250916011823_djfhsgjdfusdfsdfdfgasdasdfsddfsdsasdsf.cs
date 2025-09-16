using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class djfhsgjdfusdfsdfdfgasdasdfsddfsdsasdsf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_ProblemLists_ProblemListId",
                table: "Goals");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PsyProblemLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProblemListId",
                table: "Goals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PsyProblemListId",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PsychiatricAssessmentId",
                table: "Goals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_PsychiatricAssessmentId",
                table: "Goals",
                column: "PsychiatricAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_PsyProblemListId",
                table: "Goals",
                column: "PsyProblemListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_ProblemLists_ProblemListId",
                table: "Goals",
                column: "ProblemListId",
                principalTable: "ProblemLists",
                principalColumn: "ProblemListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_PsyProblemLists_PsyProblemListId",
                table: "Goals",
                column: "PsyProblemListId",
                principalTable: "PsyProblemLists",
                principalColumn: "PsyProblemListId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_PsychiatricAssessments_PsychiatricAssessmentId",
                table: "Goals",
                column: "PsychiatricAssessmentId",
                principalTable: "PsychiatricAssessments",
                principalColumn: "PsychiatricAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_ProblemLists_ProblemListId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_PsyProblemLists_PsyProblemListId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_PsychiatricAssessments_PsychiatricAssessmentId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_PsychiatricAssessmentId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_PsyProblemListId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PsyProblemLists");

            migrationBuilder.DropColumn(
                name: "PsyProblemListId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "PsychiatricAssessmentId",
                table: "Goals");

            migrationBuilder.AlterColumn<int>(
                name: "ProblemListId",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_ProblemLists_ProblemListId",
                table: "Goals",
                column: "ProblemListId",
                principalTable: "ProblemLists",
                principalColumn: "ProblemListId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
