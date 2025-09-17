using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class MakePsyProblemListIdNullablejhgsasdasdasdsdfsdfasdasdasdxcvxvsdfsdsdfsdasdasdasdfsdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationOrders_PsyProblemLists_PsyProblemList",
                table: "MedicationOrders");

            migrationBuilder.RenameColumn(
                name: "PsyProblemList",
                table: "MedicationOrders",
                newName: "PsyProblemListId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationOrders_PsyProblemList",
                table: "MedicationOrders",
                newName: "IX_MedicationOrders_PsyProblemListId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationOrders_PsyProblemLists_PsyProblemListId",
                table: "MedicationOrders",
                column: "PsyProblemListId",
                principalTable: "PsyProblemLists",
                principalColumn: "PsyProblemListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationOrders_PsyProblemLists_PsyProblemListId",
                table: "MedicationOrders");

            migrationBuilder.RenameColumn(
                name: "PsyProblemListId",
                table: "MedicationOrders",
                newName: "PsyProblemList");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationOrders_PsyProblemListId",
                table: "MedicationOrders",
                newName: "IX_MedicationOrders_PsyProblemList");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationOrders_PsyProblemLists_PsyProblemList",
                table: "MedicationOrders",
                column: "PsyProblemList",
                principalTable: "PsyProblemLists",
                principalColumn: "PsyProblemListId");
        }
    }
}
