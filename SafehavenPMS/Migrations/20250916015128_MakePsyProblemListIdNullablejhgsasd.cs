using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class MakePsyProblemListIdNullablejhgsasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PsyProblemLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PsyProblemLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PsyProblemLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PsyProblemLists",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PsyProblemLists");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PsyProblemLists");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PsyProblemLists");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PsyProblemLists");
        }
    }
}
