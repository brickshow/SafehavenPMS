using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalStaffs",
                columns: table => new
                {
                    ClinicalStaffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePictureURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PRC_Licensed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalStaffs", x => x.ClinicalStaffID);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    MedicineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GenericName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrandName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Strength = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.MedicineId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Occupation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfReferral = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferredBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Affiliation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "Availabilities",
                columns: table => new
                {
                    AvailabilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicalStaffID = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    SlotDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Availabilities", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_Availabilities_ClinicalStaffs_ClinicalStaffID",
                        column: x => x.ClinicalStaffID,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admissions",
                columns: table => new
                {
                    AdmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PhysicianId = table.Column<int>(type: "int", nullable: true),
                    PsychiatristId = table.Column<int>(type: "int", nullable: true),
                    PsychologistId = table.Column<int>(type: "int", nullable: true),
                    PsychometricianId = table.Column<int>(type: "int", nullable: true),
                    SocialWorkerId = table.Column<int>(type: "int", nullable: true),
                    RecoveryCoachId = table.Column<int>(type: "int", nullable: true),
                    FamilyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FamilyRelationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FamilyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FamilyEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActivatePortal = table.Column<bool>(type: "bit", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissions", x => x.AdmissionId);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_PhysicianId",
                        column: x => x.PhysicianId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_PsychiatristId",
                        column: x => x.PsychiatristId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_PsychometricianId",
                        column: x => x.PsychometricianId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_RecoveryCoachId",
                        column: x => x.RecoveryCoachId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_SocialWorkerId",
                        column: x => x.SocialWorkerId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalStaffPatients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ClinicalStaffId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalStaffPatients", x => new { x.PatientId, x.ClinicalStaffId });
                    table.ForeignKey(
                        name: "FK_ClinicalStaffPatients_ClinicalStaffs_ClinicalStaffId",
                        column: x => x.ClinicalStaffId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalStaffPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicationOrders",
                columns: table => new
                {
                    MedicationOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Dose = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationOrders", x => x.MedicationOrderId);
                    table.ForeignKey(
                        name: "FK_MedicationOrders_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicationOrders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewAppointments",
                columns: table => new
                {
                    AppointmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicalStaffID = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    TimeSlot = table.Column<TimeSpan>(type: "time", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VisitType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewAppointments", x => x.AppointmentID);
                    table.ForeignKey(
                        name: "FK_NewAppointments_ClinicalStaffs_ClinicalStaffID",
                        column: x => x.ClinicalStaffID,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewAppointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PatientId",
                table: "Admissions",
                column: "PatientId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_ClinicalStaffID",
                table: "Availabilities",
                column: "ClinicalStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalStaffPatients_ClinicalStaffId",
                table: "ClinicalStaffPatients",
                column: "ClinicalStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationOrders_MedicineId",
                table: "MedicationOrders",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationOrders_PatientId",
                table: "MedicationOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_NewAppointments_ClinicalStaffID",
                table: "NewAppointments",
                column: "ClinicalStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_NewAppointments_PatientId",
                table: "NewAppointments",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "Availabilities");

            migrationBuilder.DropTable(
                name: "ClinicalStaffPatients");

            migrationBuilder.DropTable(
                name: "MedicationOrders");

            migrationBuilder.DropTable(
                name: "NewAppointments");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "ClinicalStaffs");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
