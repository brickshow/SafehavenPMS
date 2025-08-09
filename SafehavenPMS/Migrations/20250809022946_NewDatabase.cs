using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class NewDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicalStaffs_Addresses_AddressID",
                table: "ClinicalStaffs");

            migrationBuilder.DropForeignKey(
                name: "FK_ClinicalStaffs_Addresses_AddressID1",
                table: "ClinicalStaffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Addresses_AddressID",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Addresses_AddressID1",
                table: "Patients");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Patients_AddressID",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_AddressID1",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_ClinicalStaffs_AddressID",
                table: "ClinicalStaffs");

            migrationBuilder.DropIndex(
                name: "IX_ClinicalStaffs_AddressID1",
                table: "ClinicalStaffs");

            migrationBuilder.DropColumn(
                name: "AddressID",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AddressID1",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AddressID",
                table: "ClinicalStaffs");

            migrationBuilder.DropColumn(
                name: "AddressID1",
                table: "ClinicalStaffs");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ClinicalStaffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "ClinicalStaffs");

            migrationBuilder.AddColumn<int>(
                name: "AddressID",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AddressID1",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AddressID",
                table: "ClinicalStaffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AddressID1",
                table: "ClinicalStaffs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barangay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    House_Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subdivision_Village = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AddressID",
                table: "Patients",
                column: "AddressID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AddressID1",
                table: "Patients",
                column: "AddressID1");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalStaffs_AddressID",
                table: "ClinicalStaffs",
                column: "AddressID");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalStaffs_AddressID1",
                table: "ClinicalStaffs",
                column: "AddressID1");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicalStaffs_Addresses_AddressID",
                table: "ClinicalStaffs",
                column: "AddressID",
                principalTable: "Addresses",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicalStaffs_Addresses_AddressID1",
                table: "ClinicalStaffs",
                column: "AddressID1",
                principalTable: "Addresses",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Addresses_AddressID",
                table: "Patients",
                column: "AddressID",
                principalTable: "Addresses",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Addresses_AddressID1",
                table: "Patients",
                column: "AddressID1",
                principalTable: "Addresses",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
