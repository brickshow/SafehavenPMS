using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class sdhgdfgdfsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationOrder_Medicines_MedicineId",
                table: "MedicationOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationOrder_Patients_PatientId",
                table: "MedicationOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicationOrder",
                table: "MedicationOrder");

            migrationBuilder.RenameTable(
                name: "MedicationOrder",
                newName: "MedicationOrders");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationOrder_PatientId",
                table: "MedicationOrders",
                newName: "IX_MedicationOrders_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationOrder_MedicineId",
                table: "MedicationOrders",
                newName: "IX_MedicationOrders_MedicineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicationOrders",
                table: "MedicationOrders",
                column: "MedicationOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationOrders_Medicines_MedicineId",
                table: "MedicationOrders",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationOrders_Patients_PatientId",
                table: "MedicationOrders",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationOrders_Medicines_MedicineId",
                table: "MedicationOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationOrders_Patients_PatientId",
                table: "MedicationOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicationOrders",
                table: "MedicationOrders");

            migrationBuilder.RenameTable(
                name: "MedicationOrders",
                newName: "MedicationOrder");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationOrders_PatientId",
                table: "MedicationOrder",
                newName: "IX_MedicationOrder_PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicationOrders_MedicineId",
                table: "MedicationOrder",
                newName: "IX_MedicationOrder_MedicineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicationOrder",
                table: "MedicationOrder",
                column: "MedicationOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationOrder_Medicines_MedicineId",
                table: "MedicationOrder",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationOrder_Patients_PatientId",
                table: "MedicationOrder",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
