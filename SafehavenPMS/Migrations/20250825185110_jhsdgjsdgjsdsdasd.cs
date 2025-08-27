using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class jhsdgjsdgjsdsdasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdministrationLogs",
                columns: table => new
                {
                    AdministrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicationOrderId = table.Column<int>(type: "int", nullable: false),
                    AdministrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakfastTaken = table.Column<bool>(type: "bit", nullable: false),
                    LunchTaken = table.Column<bool>(type: "bit", nullable: false),
                    DinnerTaken = table.Column<bool>(type: "bit", nullable: false),
                    BedtimeTaken = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrationLogs", x => x.AdministrationId);
                    table.ForeignKey(
                        name: "FK_AdministrationLogs_MedicationOrders_MedicationOrderId",
                        column: x => x.MedicationOrderId,
                        principalTable: "MedicationOrders",
                        principalColumn: "MedicationOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdministrationLogs_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId");
                    table.ForeignKey(
                        name: "FK_AdministrationLogs_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationLogs_MedicationOrderId",
                table: "AdministrationLogs",
                column: "MedicationOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationLogs_MedicineId",
                table: "AdministrationLogs",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationLogs_PatientId",
                table: "AdministrationLogs",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministrationLogs");
        }
    }
}
