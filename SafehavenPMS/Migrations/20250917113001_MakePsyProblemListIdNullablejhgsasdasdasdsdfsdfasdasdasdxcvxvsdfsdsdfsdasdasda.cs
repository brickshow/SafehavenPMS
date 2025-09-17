using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class MakePsyProblemListIdNullablejhgsasdasdasdsdfsdfasdasdasdxcvxvsdfsdsdfsdasdasda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PsyProblemList",
                table: "MedicationOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicationOrders_PsyProblemList",
                table: "MedicationOrders",
                column: "PsyProblemList");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationOrders_PsyProblemLists_PsyProblemList",
                table: "MedicationOrders",
                column: "PsyProblemList",
                principalTable: "PsyProblemLists",
                principalColumn: "PsyProblemListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationOrders_PsyProblemLists_PsyProblemList",
                table: "MedicationOrders");

            migrationBuilder.DropIndex(
                name: "IX_MedicationOrders_PsyProblemList",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "PsyProblemList",
                table: "MedicationOrders");
        }
    }
}
