using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class MakePsyProblemListIdNullablejhgsasdasdasdsdfsdfasdasdasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeForms_Patients_PatientId",
                table: "IntakeForms");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeForms_Patients_PatientId",
                table: "IntakeForms",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntakeForms_Patients_PatientId",
                table: "IntakeForms");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeForms_Patients_PatientId",
                table: "IntakeForms",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
