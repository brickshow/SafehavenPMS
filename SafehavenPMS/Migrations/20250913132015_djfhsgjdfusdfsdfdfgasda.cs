using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class djfhsgjdfusdfsdfdfgasda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_ClinicalStaffID",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_ClinicalStaffID",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "ActivatePortal",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "ClinicalStaffID",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "CurrentFacility",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "FamilyEmail",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "FamilyName",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "FamilyPhone",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "FamilyRelationship",
                table: "Admissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActivatePortal",
                table: "Admissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ClinicalStaffID",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentFacility",
                table: "Admissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyEmail",
                table: "Admissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyName",
                table: "Admissions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyPhone",
                table: "Admissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyRelationship",
                table: "Admissions",
                type: "nvarchar(100)",
                maxLength: 100,
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
    }
}
