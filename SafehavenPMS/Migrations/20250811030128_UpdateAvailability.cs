using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NoEndDate",
                table: "Availabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoEndDate",
                table: "Availabilities");
        }
    }
}
