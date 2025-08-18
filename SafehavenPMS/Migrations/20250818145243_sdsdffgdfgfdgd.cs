using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class sdsdffgdfgfdgd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Dosage",
                table: "Medicines",
                newName: "Unit");

            migrationBuilder.AddColumn<decimal>(
                name: "Strength",
                table: "Medicines",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Strength",
                table: "Medicines");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Medicines",
                newName: "Dosage");
        }
    }
}
