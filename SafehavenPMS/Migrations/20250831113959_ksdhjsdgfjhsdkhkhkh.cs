using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class ksdhjsdgfjhsdkhkhkh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherFamilyDetails",
                table: "FamilyMembers");

            migrationBuilder.AddColumn<string>(
                name: "OtherFamilyDetails",
                table: "IntakeForms",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherFamilyDetails",
                table: "IntakeForms");

            migrationBuilder.AddColumn<string>(
                name: "OtherFamilyDetails",
                table: "FamilyMembers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
