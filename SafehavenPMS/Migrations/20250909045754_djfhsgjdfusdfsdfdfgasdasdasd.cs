using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class djfhsgjdfusdfsdfdfgasdasdasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "Admissions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Endedby",
                table: "Admissions",
                newName: "EndedBy");

            migrationBuilder.AddColumn<int>(
                name: "AdmissionId",
                table: "ClinicalStaffPatients",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Admissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EndedBy",
                table: "Admissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhysicianId",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PsychologistId",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PsychometricianId",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecoveryCoachId",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SocialWorkerId",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalStaffPatients_AdmissionId",
                table: "ClinicalStaffPatients",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PhysicianId",
                table: "Admissions",
                column: "PhysicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PsychologistId",
                table: "Admissions",
                column: "PsychologistId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PsychometricianId",
                table: "Admissions",
                column: "PsychometricianId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_RecoveryCoachId",
                table: "Admissions",
                column: "RecoveryCoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_SocialWorkerId",
                table: "Admissions",
                column: "SocialWorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PhysicianId",
                table: "Admissions",
                column: "PhysicianId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychologistId",
                table: "Admissions",
                column: "PsychologistId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychometricianId",
                table: "Admissions",
                column: "PsychometricianId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_RecoveryCoachId",
                table: "Admissions",
                column: "RecoveryCoachId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_SocialWorkerId",
                table: "Admissions",
                column: "SocialWorkerId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicalStaffPatients_Admissions_AdmissionId",
                table: "ClinicalStaffPatients",
                column: "AdmissionId",
                principalTable: "Admissions",
                principalColumn: "AdmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PhysicianId",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychologistId",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychometricianId",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_RecoveryCoachId",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_SocialWorkerId",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ClinicalStaffPatients_Admissions_AdmissionId",
                table: "ClinicalStaffPatients");

            migrationBuilder.DropIndex(
                name: "IX_ClinicalStaffPatients_AdmissionId",
                table: "ClinicalStaffPatients");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_PhysicianId",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_PsychologistId",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_PsychometricianId",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_RecoveryCoachId",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_SocialWorkerId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "AdmissionId",
                table: "ClinicalStaffPatients");

            migrationBuilder.DropColumn(
                name: "PhysicianId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "PsychologistId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "PsychometricianId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "RecoveryCoachId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "SocialWorkerId",
                table: "Admissions");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Admissions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "EndedBy",
                table: "Admissions",
                newName: "Endedby");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Admissions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Endedby",
                table: "Admissions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
