using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClinicalStaffID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicalStaffId",
                table: "Availabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_ClinicalStaffId",
                table: "Availabilities",
                column: "ClinicalStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_ClinicalStaffs_ClinicalStaffId",
                table: "Availabilities",
                column: "ClinicalStaffId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_ClinicalStaffs_ClinicalStaffId",
                table: "Availabilities");

            migrationBuilder.DropIndex(
                name: "IX_Availabilities_ClinicalStaffId",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "ClinicalStaffId",
                table: "Availabilities");
        }
    }
}
