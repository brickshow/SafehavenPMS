using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Authorization;


#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
[Authorize]
    public partial class updateServiceTypeIdsdfsdasdlkjsdasdasdlkjhasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RecordedBy",
                table: "AdministrationLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RecordedBy",
                table: "AdministrationLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

