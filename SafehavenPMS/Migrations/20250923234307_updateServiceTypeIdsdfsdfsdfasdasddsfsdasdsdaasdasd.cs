using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class updateServiceTypeIdsdfsdfsdfasdasddsfsdasdsdaasdasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ClinicalStaffs_ClinicalStaffId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Firstname",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Lastname",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ClinicalStaffId",
                table: "Users",
                newName: "ClinicalStaffID");

            migrationBuilder.RenameIndex(
                name: "IX_Users_ClinicalStaffId",
                table: "Users",
                newName: "IX_Users_ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ClinicalStaffs_ClinicalStaffID",
                table: "Users",
                column: "ClinicalStaffID",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ClinicalStaffs_ClinicalStaffID",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ClinicalStaffID",
                table: "Users",
                newName: "ClinicalStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_ClinicalStaffID",
                table: "Users",
                newName: "IX_Users_ClinicalStaffId");

            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Lastname",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ClinicalStaffs_ClinicalStaffId",
                table: "Users",
                column: "ClinicalStaffId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");
        }
    }
}
