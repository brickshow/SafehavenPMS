using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class djfhsgjdfusdfsdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PhysicianId",
                table: "Admissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychiatristId",
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

            migrationBuilder.DropIndex(
                name: "IX_Admissions_PhysicianId",
                table: "Admissions");

            migrationBuilder.DropIndex(
                name: "IX_Admissions_PsychiatristId",
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
                name: "Diagnosis",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "IsDrugDependent",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "PhysicianId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "PsychiatristId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "PsychologistId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "PsychometricianId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "RecoveryCoachId",
                table: "Admissions");

            migrationBuilder.DropColumn(
                name: "SocialWorkerId",
                table: "Admissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "Admissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDrugDependent",
                table: "Admissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PhysicianId",
                table: "Admissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PsychiatristId",
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

            migrationBuilder.AddColumn<string>(
                name: "Recommendation",
                table: "Admissions",
                type: "nvarchar(max)",
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
                name: "IX_Admissions_PhysicianId",
                table: "Admissions",
                column: "PhysicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PsychiatristId",
                table: "Admissions",
                column: "PsychiatristId");

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
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychiatristId",
                table: "Admissions",
                column: "PsychiatristId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychologistId",
                table: "Admissions",
                column: "PsychologistId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_PsychometricianId",
                table: "Admissions",
                column: "PsychometricianId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_RecoveryCoachId",
                table: "Admissions",
                column: "RecoveryCoachId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissions_ClinicalStaffs_SocialWorkerId",
                table: "Admissions",
                column: "SocialWorkerId",
                principalTable: "ClinicalStaffs",
                principalColumn: "ClinicalStaffID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
