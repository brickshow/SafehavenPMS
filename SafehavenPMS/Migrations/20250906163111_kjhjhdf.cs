using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class kjhjhdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfReferral",
                table: "IntakeForms");

            migrationBuilder.RenameColumn(
                name: "IntakeStatus",
                table: "IntakeForms",
                newName: "UpdatedBy");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "IntakeForms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "IntakeForms",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "IntakeForms");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "IntakeForms",
                newName: "IntakeStatus");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfReferral",
                table: "IntakeForms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
