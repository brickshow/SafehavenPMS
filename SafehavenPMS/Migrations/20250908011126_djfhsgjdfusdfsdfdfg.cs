using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class djfhsgjdfusdfsdfdfg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicalStaffID",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_ClinicalStaffID",
                table: "Admissions",
                column: "ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_ClinicalStaffID",
                table: "Admissions",
                column: "ClinicalStaffID",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_ClinicalStaffID",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_ClinicalStaffID",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "ClinicalStaffID",
                table: "Admissions");
        }
    }
}
