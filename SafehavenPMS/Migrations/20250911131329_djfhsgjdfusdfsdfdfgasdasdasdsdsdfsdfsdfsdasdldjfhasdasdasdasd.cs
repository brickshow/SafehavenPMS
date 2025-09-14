using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class djfhsgjdfusdfsdfdfgasdasdasdsdsdfsdfsdfsdasdldjfhasdasdasdasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PsyProblemLists",
                columns: table => new
                {
                    PsyProblemListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PsychiatricAssessmentId = table.Column<int>(type: "int", nullable: false),
                    Problem = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsyProblemLists", x => x.PsyProblemListId);
                    table.ForeignKey(
                        name: "FK_PsyProblemLists_PsychiatricAssessments_PsychiatricAssessmentId",
                        column: x => x.PsychiatricAssessmentId,
                        principalTable: "PsychiatricAssessments",
                        principalColumn: "PsychiatricAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PsyProblemLists_PsychiatricAssessmentId",
                table: "PsyProblemLists",
                column: "PsychiatricAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PsyProblemLists");
        }
    }
}
