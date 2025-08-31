using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class MergeIntakeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientIntakes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "IntakeForms",
                newName: "IntakeFormsId");

            migrationBuilder.AlterColumn<int>(
                name: "IntakeFormId",
                table: "PresentingProblems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PatientIntakeIntakeFormsId",
                table: "PresentingProblems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Affiliation",
                table: "IntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfReferral",
                table: "IntakeForms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IntakeStatus",
                table: "IntakeForms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "IntakeForms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "IntakeForms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PresentingComplaint",
                table: "IntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferredBy",
                table: "IntakeForms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PatientIntakeIntakeFormsId",
                table: "FamilyMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IntakeFormId",
                table: "CounselorImpressions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PatientIntakeIntakeFormsId",
                table: "CounselorImpressions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PresentingProblems_PatientIntakeIntakeFormsId",
                table: "PresentingProblems",
                column: "PatientIntakeIntakeFormsId");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeForms_PatientId",
                table: "IntakeForms",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_PatientIntakeIntakeFormsId",
                table: "FamilyMembers",
                column: "PatientIntakeIntakeFormsId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselorImpressions_PatientIntakeIntakeFormsId",
                table: "CounselorImpressions",
                column: "PatientIntakeIntakeFormsId");

            migrationBuilder.AddForeignKey(
                name: "FK_CounselorImpressions_IntakeForms_PatientIntakeIntakeFormsId",
                table: "CounselorImpressions",
                column: "PatientIntakeIntakeFormsId",
                principalTable: "IntakeForms",
                principalColumn: "IntakeFormsId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_IntakeForms_PatientIntakeIntakeFormsId",
                table: "FamilyMembers",
                column: "PatientIntakeIntakeFormsId",
                principalTable: "IntakeForms",
                principalColumn: "IntakeFormsId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntakeForms_Patients_PatientId",
                table: "IntakeForms",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PresentingProblems_IntakeForms_PatientIntakeIntakeFormsId",
                table: "PresentingProblems",
                column: "PatientIntakeIntakeFormsId",
                principalTable: "IntakeForms",
                principalColumn: "IntakeFormsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CounselorImpressions_IntakeForms_PatientIntakeIntakeFormsId",
                table: "CounselorImpressions");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_IntakeForms_PatientIntakeIntakeFormsId",
                table: "FamilyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_IntakeForms_Patients_PatientId",
                table: "IntakeForms");

            migrationBuilder.DropForeignKey(
                name: "FK_PresentingProblems_IntakeForms_PatientIntakeIntakeFormsId",
                table: "PresentingProblems");

            migrationBuilder.DropIndex(
                name: "IX_PresentingProblems_PatientIntakeIntakeFormsId",
                table: "PresentingProblems");

            migrationBuilder.DropIndex(
                name: "IX_IntakeForms_PatientId",
                table: "IntakeForms");

            migrationBuilder.DropIndex(
                name: "IX_FamilyMembers_PatientIntakeIntakeFormsId",
                table: "FamilyMembers");

            migrationBuilder.DropIndex(
                name: "IX_CounselorImpressions_PatientIntakeIntakeFormsId",
                table: "CounselorImpressions");

            migrationBuilder.DropColumn(
                name: "PatientIntakeIntakeFormsId",
                table: "PresentingProblems");

            migrationBuilder.DropColumn(
                name: "Affiliation",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "DateOfReferral",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "IntakeStatus",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "PresentingComplaint",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "ReferredBy",
                table: "IntakeForms");

            migrationBuilder.DropColumn(
                name: "PatientIntakeIntakeFormsId",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "PatientIntakeIntakeFormsId",
                table: "CounselorImpressions");

            migrationBuilder.RenameColumn(
                name: "IntakeFormsId",
                table: "IntakeForms",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "IntakeFormId",
                table: "PresentingProblems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IntakeFormId",
                table: "CounselorImpressions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "PatientIntakes",
                columns: table => new
                {
                    PatientIntakeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Affiliation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfReferral = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntakeStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PresentingComplaint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferredBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIntakes", x => x.PatientIntakeId);
                    table.ForeignKey(
                        name: "FK_PatientIntakes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_PatientId",
                table: "PatientIntakes",
                column: "PatientId",
                unique: true);
        }
    }
}
