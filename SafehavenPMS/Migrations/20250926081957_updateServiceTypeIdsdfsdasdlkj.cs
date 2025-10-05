using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Authorization;


#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
[Authorize]
    public partial class updateServiceTypeIdsdfsdasdlkj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProofFileName",
                table: "Payments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProofFileName",
                table: "Payments");
        }
    }
}

