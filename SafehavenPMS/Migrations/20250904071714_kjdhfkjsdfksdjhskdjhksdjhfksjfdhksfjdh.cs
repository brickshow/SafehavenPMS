using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class kjdhfkjsdfksdjhskdjhksdjhfksjfdhksfjdh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhysicalExams",
                columns: table => new
                {
                    PhysicalExamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    BP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Temperature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    O2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkinNormal = table.Column<bool>(type: "bit", nullable: false),
                    SkinFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ENTNormal = table.Column<bool>(type: "bit", nullable: false),
                    ENTFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChestNormal = table.Column<bool>(type: "bit", nullable: false),
                    ChestFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LungsNormal = table.Column<bool>(type: "bit", nullable: false),
                    LungsFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSNormal = table.Column<bool>(type: "bit", nullable: false),
                    CVSFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbdomenNormal = table.Column<bool>(type: "bit", nullable: false),
                    AbdomenFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GUTNormal = table.Column<bool>(type: "bit", nullable: false),
                    GUTFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtremitiesNormal = table.Column<bool>(type: "bit", nullable: false),
                    ExtremitiesFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalExams", x => x.PhysicalExamId);
                    table.ForeignKey(
                        name: "FK_PhysicalExams_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalExams_InitialAssessmentFormId",
                table: "PhysicalExams",
                column: "InitialAssessmentFormId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhysicalExams");
        }
    }
}
