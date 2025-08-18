using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class sdsdffgdfgfdgdsdfsdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MedicineName",
                table: "Medicines",
                newName: "GenericName");

            migrationBuilder.AddColumn<string>(
                name: "BrandName",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "Medicines");

            migrationBuilder.RenameColumn(
                name: "GenericName",
                table: "Medicines",
                newName: "MedicineName");
        }
    }
}
