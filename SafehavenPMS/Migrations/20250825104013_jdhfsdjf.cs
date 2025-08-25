using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class jdhfsdjf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dose",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "MedicationOrders");

            migrationBuilder.RenameColumn(
                name: "Instruction",
                table: "MedicationOrders",
                newName: "ScheduledType");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "MedicationOrders",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<bool>(
                name: "Bedtime",
                table: "MedicationOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Breakfast",
                table: "MedicationOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MedicationOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysInterval",
                table: "MedicationOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Dinner",
                table: "MedicationOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscontinueDate",
                table: "MedicationOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Lunch",
                table: "MedicationOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NoDiscontinueDate",
                table: "MedicationOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "MedicationOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitPerDose",
                table: "MedicationOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedicationOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MedicationOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bedtime",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "Breakfast",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "DaysInterval",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "Dinner",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "DiscontinueDate",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "Lunch",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "NoDiscontinueDate",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "UnitPerDose",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MedicationOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicationOrders");

            migrationBuilder.RenameColumn(
                name: "ScheduledType",
                table: "MedicationOrders",
                newName: "Instruction");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "MedicationOrders",
                newName: "EndDate");

            migrationBuilder.AddColumn<decimal>(
                name: "Dose",
                table: "MedicationOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "MedicationOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
