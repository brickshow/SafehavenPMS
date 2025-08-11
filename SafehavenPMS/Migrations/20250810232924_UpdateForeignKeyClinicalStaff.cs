using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForeignKeyClinicalStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_ClinicalStaffs_ClinicalStaffId",
                table: "Availabilities");

            migrationBuilder.RenameColumn(
                name: "ClinicalStaffId",
                table: "Availabilities",
                newName: "ClinicalStaffID");

            migrationBuilder.RenameIndex(
                name: "IX_Availabilities_ClinicalStaffId",
                table: "Availabilities",
                newName: "IX_Availabilities_ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_ClinicalStaffs_ClinicalStaffID",
                table: "Availabilities",
                column: "ClinicalStaffID",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_ClinicalStaffs_ClinicalStaffID",
                table: "Availabilities");

            migrationBuilder.RenameColumn(
                name: "ClinicalStaffID",
                table: "Availabilities",
                newName: "ClinicalStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Availabilities_ClinicalStaffID",
                table: "Availabilities",
                newName: "IX_Availabilities_ClinicalStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_ClinicalStaffs_ClinicalStaffId",
                table: "Availabilities",
                column: "ClinicalStaffId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
