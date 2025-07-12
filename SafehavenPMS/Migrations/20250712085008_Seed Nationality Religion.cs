using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class SeedNationalityReligion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Religion",
                table: "Patients");

            migrationBuilder.AddColumn<int>(
                name: "AddressID",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NationalityID",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReligionID",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    AddressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Barangay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressID);
                });

            migrationBuilder.CreateTable(
                name: "Nationalities",
                columns: table => new
                {
                    NationalityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalityName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationalities", x => x.NationalityID);
                });

            migrationBuilder.CreateTable(
                name: "Religions",
                columns: table => new
                {
                    ReligionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReligionName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Religions", x => x.ReligionID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AddressID",
                table: "Patients",
                column: "AddressID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalityID",
                table: "Patients",
                column: "NationalityID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ReligionID",
                table: "Patients",
                column: "ReligionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Address_AddressID",
                table: "Patients",
                column: "AddressID",
                principalTable: "Address",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Nationalities_NationalityID",
                table: "Patients",
                column: "NationalityID",
                principalTable: "Nationalities",
                principalColumn: "NationalityID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Religions_ReligionID",
                table: "Patients",
                column: "ReligionID",
                principalTable: "Religions",
                principalColumn: "ReligionID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Address_AddressID",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Nationalities_NationalityID",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Religions_ReligionID",
                table: "Patients");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Nationalities");

            migrationBuilder.DropTable(
                name: "Religions");

            migrationBuilder.DropIndex(
                name: "IX_Patients_AddressID",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_NationalityID",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ReligionID",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AddressID",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "NationalityID",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReligionID",
                table: "Patients");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Religion",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
