using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class updateServiceTypeIdsdfsdfsdfasdasdds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferenceType",
                table: "Billables",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Billables_PatientId",
                table: "Billables",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Billables_Patients_PatientId",
                table: "Billables",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billables_Patients_PatientId",
                table: "Billables");

            migrationBuilder.DropIndex(
                name: "IX_Billables_PatientId",
                table: "Billables");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceType",
                table: "Billables",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
