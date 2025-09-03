using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class kasdjhkajdhkahdkajd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewAppointments_ClinicalStaffs_ClinicalStaffID",
                table: "NewAppointments");

            migrationBuilder.DropTable(
                name: "Schedulings");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "NewAppointments");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "NewAppointments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "NewAppointments");

            migrationBuilder.DropColumn(
                name: "TimeSlot",
                table: "NewAppointments");

            migrationBuilder.RenameColumn(
                name: "VisitType",
                table: "NewAppointments",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "BookedAt",
                table: "NewAppointments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "AppointmentID",
                table: "NewAppointments",
                newName: "ScheduleId");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicalStaffID",
                table: "NewAppointments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "NewAppointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "NewAppointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleDate",
                table: "NewAppointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleTime",
                table: "NewAppointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NewAppointments_ClinicalStaffs_ClinicalStaffID",
                table: "NewAppointments",
                column: "ClinicalStaffID",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewAppointments_ClinicalStaffs_ClinicalStaffID",
                table: "NewAppointments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "NewAppointments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "NewAppointments");

            migrationBuilder.DropColumn(
                name: "ScheduleDate",
                table: "NewAppointments");

            migrationBuilder.DropColumn(
                name: "ScheduleTime",
                table: "NewAppointments");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "NewAppointments",
                newName: "VisitType");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "NewAppointments",
                newName: "BookedAt");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "NewAppointments",
                newName: "AppointmentID");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicalStaffID",
                table: "NewAppointments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "NewAppointments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "NewAppointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "NewAppointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeSlot",
                table: "NewAppointments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateTable(
                name: "Schedulings",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicalStaffID = table.Column<int>(type: "int", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedulings", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedulings_ClinicalStaffs_ClinicalStaffID",
                        column: x => x.ClinicalStaffID,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedulings_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedulings_ClinicalStaffID",
                table: "Schedulings",
                column: "ClinicalStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Schedulings_PatientId",
                table: "Schedulings",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewAppointments_ClinicalStaffs_ClinicalStaffID",
                table: "NewAppointments",
                column: "ClinicalStaffID",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
