using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class rf4rf4r : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_PatientIntakes_PatientIntakeId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PatientIntakeId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PatientIntakeId",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_PatientId",
                table: "PatientIntakes",
                column: "PatientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientIntakes_Patients_PatientId",
                table: "PatientIntakes",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientIntakes_Patients_PatientId",
                table: "PatientIntakes");

            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_PatientId",
                table: "PatientIntakes");

            migrationBuilder.AddColumn<int>(
                name: "PatientIntakeId",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientIntakeId",
                table: "Patients",
                column: "PatientIntakeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_PatientIntakes_PatientIntakeId",
                table: "Patients",
                column: "PatientIntakeId",
                principalTable: "PatientIntakes",
                principalColumn: "PatientIntakeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
