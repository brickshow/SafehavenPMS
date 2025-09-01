using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class kjhuijhk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedulings_ClinicalStaffs_DoctorId",
                table: "Schedulings");

            migrationBuilder.DropIndex(
                name: "IX_Schedulings_DoctorId",
                table: "Schedulings");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Schedulings");

            migrationBuilder.AddColumn<int>(
                name: "ClinicalStaffID",
                table: "Schedulings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedulings_ClinicalStaffID",
                table: "Schedulings",
                column: "ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedulings_ClinicalStaffs_ClinicalStaffID",
                table: "Schedulings",
                column: "ClinicalStaffID",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedulings_ClinicalStaffs_ClinicalStaffID",
                table: "Schedulings");

            migrationBuilder.DropIndex(
                name: "IX_Schedulings_ClinicalStaffID",
                table: "Schedulings");

            migrationBuilder.DropColumn(
                name: "ClinicalStaffID",
                table: "Schedulings");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "Schedulings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Schedulings_DoctorId",
                table: "Schedulings",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedulings_ClinicalStaffs_DoctorId",
                table: "Schedulings",
                column: "DoctorId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
