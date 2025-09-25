using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafehavenPMS.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate : Migration
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
                    BrandName = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTypes",
                columns: table => new
                {
                    ServiceTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTypes", x => x.ServiceTypeId);
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
                    CaseId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PhysicianId = table.Column<int>(type: "int", nullable: true),
                    PsychologistId = table.Column<int>(type: "int", nullable: true),
                    PsychometricianId = table.Column<int>(type: "int", nullable: true),
                    SocialWorkerId = table.Column<int>(type: "int", nullable: true),
                    RecoveryCoachId = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissions", x => x.AdmissionId);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_PhysicianId",
                        column: x => x.PhysicianId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_PsychologistId",
                        column: x => x.PsychologistId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_PsychometricianId",
                        column: x => x.PsychometricianId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_RecoveryCoachId",
                        column: x => x.RecoveryCoachId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_SocialWorkerId",
                        column: x => x.SocialWorkerId,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
                    table.ForeignKey(
                        name: "FK_Admissions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_Patients_PatientId1",
                        column: x => x.PatientId1,
                        principalTable: "Patients",
                        principalColumn: "PatientId");
                });

            migrationBuilder.CreateTable(
                name: "Billables",
                columns: table => new
                {
                    BillableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billables", x => x.BillableId);
                    table.ForeignKey(
                        name: "FK_Billables_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DischargedPatients",
                columns: table => new
                {
                    DischargeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DischargeDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DischargedPatients", x => x.DischargeId);
                    table.ForeignKey(
                        name: "FK_DischargedPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InitialAssessmentForms",
                columns: table => new
                {
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PatientId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialAssessmentForms", x => x.InitialAssessmentFormId);
                    table.ForeignKey(
                        name: "FK_InitialAssessmentForms_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InitialAssessmentForms_Patients_PatientId1",
                        column: x => x.PatientId1,
                        principalTable: "Patients",
                        principalColumn: "PatientId");
                });

            migrationBuilder.CreateTable(
                name: "IntakeForms",
                columns: table => new
                {
                    IntakeFormsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    AccompaniedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Affiliation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProblemPresentation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CouncilorImpression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherFamilyDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentingComplaint = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeForms", x => x.IntakeFormsId);
                    table.ForeignKey(
                        name: "FK_IntakeForms_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MiscellaneousItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscellaneousItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MiscellaneousItems_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewAppointments",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ClinicalStaffID = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewAppointments", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_NewAppointments_ClinicalStaffs_ClinicalStaffID",
                        column: x => x.ClinicalStaffID,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
                    table.ForeignKey(
                        name: "FK_NewAppointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PsychiatricAssessments",
                columns: table => new
                {
                    PsychiatricAssessmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistoryOfPresentIllness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalAndFamilyHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MentalStatusExamination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Impression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsychiatricAssessments", x => x.PsychiatricAssessmentId);
                    table.ForeignKey(
                        name: "FK_PsychiatricAssessments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    ClinicalStaffID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_ClinicalStaffs_ClinicalStaffID",
                        column: x => x.ClinicalStaffID,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
                    table.ForeignKey(
                        name: "FK_Users_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId");
                });

            migrationBuilder.CreateTable(
                name: "ClinicalStaffPatients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ClinicalStaffId = table.Column<int>(type: "int", nullable: false),
                    AdmissionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalStaffPatients", x => new { x.PatientId, x.ClinicalStaffId });
                    table.ForeignKey(
                        name: "FK_ClinicalStaffPatients_Admissions_AdmissionId",
                        column: x => x.AdmissionId,
                        principalTable: "Admissions",
                        principalColumn: "AdmissionId");
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
                name: "Diagnoses",
                columns: table => new
                {
                    DiagnosisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnoses", x => x.DiagnosisId);
                    table.ForeignKey(
                        name: "FK_Diagnoses_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugUses",
                columns: table => new
                {
                    DrugUseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    SubstanceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuantityPerDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstUse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectsWhenHigh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectsWhenWanes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugUses", x => x.DrugUseId);
                    table.ForeignKey(
                        name: "FK_DrugUses_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPresents",
                columns: table => new
                {
                    HistoryPresentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    OnsetOfDrugUse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonForFirstUse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistoryOfImprisonment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousDrugRehab = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhoInvitedFirstUse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfPeopleFirstUse = table.Column<int>(type: "int", nullable: true),
                    LastUseOfSubstance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmountConsumedFirstUse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPresents", x => x.HistoryPresentId);
                    table.ForeignKey(
                        name: "FK_HistoryPresents_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAllergies",
                columns: table => new
                {
                    MedicalAllergyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    AllergyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllergyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAllergies", x => x.MedicalAllergyId);
                    table.ForeignKey(
                        name: "FK_MedicalAllergies_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    MedicalHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    IsHypertensive = table.Column<bool>(type: "bit", nullable: false),
                    IsDiabetic = table.Column<bool>(type: "bit", nullable: false),
                    IsAsthmatic = table.Column<bool>(type: "bit", nullable: false),
                    OtherConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaternalHypertension = table.Column<bool>(type: "bit", nullable: false),
                    MaternalDiabetic = table.Column<bool>(type: "bit", nullable: false),
                    MaternalNone = table.Column<bool>(type: "bit", nullable: false),
                    PaternalHypertension = table.Column<bool>(type: "bit", nullable: false),
                    PaternalDiabetic = table.Column<bool>(type: "bit", nullable: false),
                    PaternalNone = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.MedicalHistoryId);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentalStatusExaminations",
                columns: table => new
                {
                    MentalStatusExaminationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    GeneralAppearanceNeat = table.Column<bool>(type: "bit", nullable: false),
                    GeneralAppearanceDishevelled = table.Column<bool>(type: "bit", nullable: false),
                    GeneralAppearanceInappropriate = table.Column<bool>(type: "bit", nullable: false),
                    GeneralAppearanceOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeechNormal = table.Column<bool>(type: "bit", nullable: false),
                    SpeechRapid = table.Column<bool>(type: "bit", nullable: false),
                    SpeechSlow = table.Column<bool>(type: "bit", nullable: false),
                    SpeechIncoherent = table.Column<bool>(type: "bit", nullable: false),
                    SpeechOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BehaviorRelaxed = table.Column<bool>(type: "bit", nullable: false),
                    BehaviorCooperative = table.Column<bool>(type: "bit", nullable: false),
                    BehaviorSuspicious = table.Column<bool>(type: "bit", nullable: false),
                    BehaviorPreoccupied = table.Column<bool>(type: "bit", nullable: false),
                    BehaviorOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViolenceRelaxed = table.Column<bool>(type: "bit", nullable: false),
                    ViolenceRestless = table.Column<bool>(type: "bit", nullable: false),
                    ViolenceClenchedFist = table.Column<bool>(type: "bit", nullable: false),
                    ViolenceRaisedVoice = table.Column<bool>(type: "bit", nullable: false),
                    ViolenceOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoodSad = table.Column<bool>(type: "bit", nullable: false),
                    MoodAnxious = table.Column<bool>(type: "bit", nullable: false),
                    MoodHappy = table.Column<bool>(type: "bit", nullable: false),
                    MoodFearful = table.Column<bool>(type: "bit", nullable: false),
                    MoodHelpless = table.Column<bool>(type: "bit", nullable: false),
                    MoodHopeless = table.Column<bool>(type: "bit", nullable: false),
                    MoodAngry = table.Column<bool>(type: "bit", nullable: false),
                    MoodOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectAppropriate = table.Column<bool>(type: "bit", nullable: false),
                    AffectInappropriate = table.Column<bool>(type: "bit", nullable: false),
                    AffectFlat = table.Column<bool>(type: "bit", nullable: false),
                    AffectBlunted = table.Column<bool>(type: "bit", nullable: false),
                    AffectOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoughtsNormal = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtsFlightOfIdeas = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtsPreoccupied = table.Column<bool>(type: "bit", nullable: false),
                    ThoughtsOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CognitionConscious = table.Column<bool>(type: "bit", nullable: false),
                    CognitionConfused = table.Column<bool>(type: "bit", nullable: false),
                    CognitionDrowsy = table.Column<bool>(type: "bit", nullable: false),
                    CognitionOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerceptionsIllusions = table.Column<bool>(type: "bit", nullable: false),
                    PerceptionsAuditoryHallucinations = table.Column<bool>(type: "bit", nullable: false),
                    PerceptionsVisualHallucinations = table.Column<bool>(type: "bit", nullable: false),
                    PerceptionsDelusions = table.Column<bool>(type: "bit", nullable: false),
                    PerceptionsParanoia = table.Column<bool>(type: "bit", nullable: false),
                    PerceptionsSuicidalAttempt = table.Column<bool>(type: "bit", nullable: false),
                    PerceptionsSuicidalIdeations = table.Column<bool>(type: "bit", nullable: false),
                    PerceptionsOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MemoryShortTerm = table.Column<bool>(type: "bit", nullable: false),
                    MemoryLongTerm = table.Column<bool>(type: "bit", nullable: false),
                    MemoryOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrientationOrientedToTime = table.Column<bool>(type: "bit", nullable: false),
                    OrientationOrientedToPerson = table.Column<bool>(type: "bit", nullable: false),
                    OrientationOrientedToPlace = table.Column<bool>(type: "bit", nullable: false),
                    OrientationDisorientedToTime = table.Column<bool>(type: "bit", nullable: false),
                    OrientationDisorientedToPerson = table.Column<bool>(type: "bit", nullable: false),
                    OrientationDisorientedToPlace = table.Column<bool>(type: "bit", nullable: false),
                    OrientationOthers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JudgementGood = table.Column<bool>(type: "bit", nullable: false),
                    JudgementFair = table.Column<bool>(type: "bit", nullable: false),
                    JudgementPoor = table.Column<bool>(type: "bit", nullable: false),
                    InsightGood = table.Column<bool>(type: "bit", nullable: false),
                    InsightFair = table.Column<bool>(type: "bit", nullable: false),
                    InsightPoor = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentalStatusExaminations", x => x.MentalStatusExaminationId);
                    table.ForeignKey(
                        name: "FK_MentalStatusExaminations_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalExams",
                columns: table => new
                {
                    PhysicalExamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    BP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Temperature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    O2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkinNormal = table.Column<bool>(type: "bit", nullable: false),
                    SkinFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ENTNormal = table.Column<bool>(type: "bit", nullable: false),
                    ENTFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChestNormal = table.Column<bool>(type: "bit", nullable: false),
                    ChestFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LungsNormal = table.Column<bool>(type: "bit", nullable: false),
                    LungsFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSNormal = table.Column<bool>(type: "bit", nullable: false),
                    CVSFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbdomenNormal = table.Column<bool>(type: "bit", nullable: false),
                    AbdomenFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GUTNormal = table.Column<bool>(type: "bit", nullable: false),
                    GUTFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtremitiesNormal = table.Column<bool>(type: "bit", nullable: false),
                    ExtremitiesFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalExams", x => x.PhysicalExamId);
                    table.ForeignKey(
                        name: "FK_PhysicalExams_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProblemLists",
                columns: table => new
                {
                    ProblemListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    Problem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemLists", x => x.ProblemListId);
                    table.ForeignKey(
                        name: "FK_ProblemLists_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    RecommendationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedDuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.RecommendationId);
                    table.ForeignKey(
                        name: "FK_Recommendations_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurgicalHistories",
                columns: table => new
                {
                    SurgicalHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitialAssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hospital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Operation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurgicalHistories", x => x.SurgicalHistoryId);
                    table.ForeignKey(
                        name: "FK_SurgicalHistories_InitialAssessmentForms_InitialAssessmentFormId",
                        column: x => x.InitialAssessmentFormId,
                        principalTable: "InitialAssessmentForms",
                        principalColumn: "InitialAssessmentFormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntakeFormId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientIntakeIntakeFormsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyMembers_IntakeForms_IntakeFormId",
                        column: x => x.IntakeFormId,
                        principalTable: "IntakeForms",
                        principalColumn: "IntakeFormsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyMembers_IntakeForms_PatientIntakeIntakeFormsId",
                        column: x => x.PatientIntakeIntakeFormsId,
                        principalTable: "IntakeForms",
                        principalColumn: "IntakeFormsId");
                });

            migrationBuilder.CreateTable(
                name: "PsyDiagnosisLists",
                columns: table => new
                {
                    PsyDiagnosisListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PsychiatricAssessmentId = table.Column<int>(type: "int", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsyDiagnosisLists", x => x.PsyDiagnosisListId);
                    table.ForeignKey(
                        name: "FK_PsyDiagnosisLists_PsychiatricAssessments_PsychiatricAssessmentId",
                        column: x => x.PsychiatricAssessmentId,
                        principalTable: "PsychiatricAssessments",
                        principalColumn: "PsychiatricAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PsyProblemLists",
                columns: table => new
                {
                    PsyProblemListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PsychiatricAssessmentId = table.Column<int>(type: "int", nullable: false),
                    Problem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsyProblemLists", x => x.PsyProblemListId);
                    table.ForeignKey(
                        name: "FK_PsyProblemLists_PsychiatricAssessments_PsychiatricAssessmentId",
                        column: x => x.PsychiatricAssessmentId,
                        principalTable: "PsychiatricAssessments",
                        principalColumn: "PsychiatricAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubstanceUseEntries",
                columns: table => new
                {
                    SubstanceUseEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiagnosisId = table.Column<int>(type: "int", nullable: false),
                    SubstanceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubstanceUseEntries", x => x.SubstanceUseEntryId);
                    table.ForeignKey(
                        name: "FK_SubstanceUseEntries_Diagnoses_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalTable: "Diagnoses",
                        principalColumn: "DiagnosisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    GoalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PsyProblemListId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProblemListId = table.Column<int>(type: "int", nullable: true),
                    PsychiatricAssessmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.GoalId);
                    table.ForeignKey(
                        name: "FK_Goals_ProblemLists_ProblemListId",
                        column: x => x.ProblemListId,
                        principalTable: "ProblemLists",
                        principalColumn: "ProblemListId");
                    table.ForeignKey(
                        name: "FK_Goals_PsyProblemLists_PsyProblemListId",
                        column: x => x.PsyProblemListId,
                        principalTable: "PsyProblemLists",
                        principalColumn: "PsyProblemListId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Goals_PsychiatricAssessments_PsychiatricAssessmentId",
                        column: x => x.PsychiatricAssessmentId,
                        principalTable: "PsychiatricAssessments",
                        principalColumn: "PsychiatricAssessmentId");
                });

            migrationBuilder.CreateTable(
                name: "Interventions",
                columns: table => new
                {
                    InterventionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PsyProblemListId = table.Column<int>(type: "int", nullable: false),
                    ServiceTypeId = table.Column<int>(type: "int", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    DurationFrequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interventions", x => x.InterventionId);
                    table.ForeignKey(
                        name: "FK_Interventions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interventions_PsyProblemLists_PsyProblemListId",
                        column: x => x.PsyProblemListId,
                        principalTable: "PsyProblemLists",
                        principalColumn: "PsyProblemListId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interventions_ServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "ServiceTypes",
                        principalColumn: "ServiceTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Interventions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicationOrders",
                columns: table => new
                {
                    MedicationOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdministrationLogId = table.Column<int>(type: "int", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    PsyProblemListId = table.Column<int>(type: "int", nullable: true),
                    UnitPerDose = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysInterval = table.Column<int>(type: "int", nullable: true),
                    Breakfast = table.Column<bool>(type: "bit", nullable: false),
                    Lunch = table.Column<bool>(type: "bit", nullable: false),
                    Dinner = table.Column<bool>(type: "bit", nullable: false),
                    Bedtime = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscontinueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoDiscontinueDate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
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
                    table.ForeignKey(
                        name: "FK_MedicationOrders_PsyProblemLists_PsyProblemListId",
                        column: x => x.PsyProblemListId,
                        principalTable: "PsyProblemLists",
                        principalColumn: "PsyProblemListId");
                });

            migrationBuilder.CreateTable(
                name: "ProgressNotes",
                columns: table => new
                {
                    ProgressNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    InterventionId = table.Column<int>(type: "int", nullable: true),
                    Clinician = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoapRaw = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Subjective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Assessment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Plan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressNotes", x => x.ProgressNoteId);
                    table.ForeignKey(
                        name: "FK_ProgressNotes_Interventions_InterventionId",
                        column: x => x.InterventionId,
                        principalTable: "Interventions",
                        principalColumn: "InterventionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressNotes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdministrationLogs",
                columns: table => new
                {
                    AdministrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicationOrderId = table.Column<int>(type: "int", nullable: false),
                    AdministrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakfastTaken = table.Column<bool>(type: "bit", nullable: false),
                    LunchTaken = table.Column<bool>(type: "bit", nullable: false),
                    DinnerTaken = table.Column<bool>(type: "bit", nullable: false),
                    BedtimeTaken = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrationLogs", x => x.AdministrationId);
                    table.ForeignKey(
                        name: "FK_AdministrationLogs_MedicationOrders_MedicationOrderId",
                        column: x => x.MedicationOrderId,
                        principalTable: "MedicationOrders",
                        principalColumn: "MedicationOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdministrationLogs_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId");
                    table.ForeignKey(
                        name: "FK_AdministrationLogs_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationLogs_MedicationOrderId",
                table: "AdministrationLogs",
                column: "MedicationOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationLogs_MedicineId",
                table: "AdministrationLogs",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationLogs_PatientId",
                table: "AdministrationLogs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PatientId",
                table: "Admissions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PatientId1",
                table: "Admissions",
                column: "PatientId1");

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

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_ClinicalStaffID",
                table: "Availabilities",
                column: "ClinicalStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Billables_PatientId",
                table: "Billables",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalStaffPatients_AdmissionId",
                table: "ClinicalStaffPatients",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalStaffPatients_ClinicalStaffId",
                table: "ClinicalStaffPatients",
                column: "ClinicalStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_InitialAssessmentFormId",
                table: "Diagnoses",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DischargedPatients_PatientId",
                table: "DischargedPatients",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugUses_InitialAssessmentFormId",
                table: "DrugUses",
                column: "InitialAssessmentFormId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_IntakeFormId",
                table: "FamilyMembers",
                column: "IntakeFormId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_PatientIntakeIntakeFormsId",
                table: "FamilyMembers",
                column: "PatientIntakeIntakeFormsId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_ProblemListId",
                table: "Goals",
                column: "ProblemListId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_PsychiatricAssessmentId",
                table: "Goals",
                column: "PsychiatricAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_PsyProblemListId",
                table: "Goals",
                column: "PsyProblemListId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPresents_InitialAssessmentFormId",
                table: "HistoryPresents",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InitialAssessmentForms_PatientId",
                table: "InitialAssessmentForms",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InitialAssessmentForms_PatientId1",
                table: "InitialAssessmentForms",
                column: "PatientId1");

            migrationBuilder.CreateIndex(
                name: "IX_IntakeForms_PatientId",
                table: "IntakeForms",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_PatientId",
                table: "Interventions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_PsyProblemListId",
                table: "Interventions",
                column: "PsyProblemListId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_ServiceId",
                table: "Interventions",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_ServiceTypeId",
                table: "Interventions",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAllergies_InitialAssessmentFormId",
                table: "MedicalAllergies",
                column: "InitialAssessmentFormId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_InitialAssessmentFormId",
                table: "MedicalHistories",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicationOrders_MedicineId",
                table: "MedicationOrders",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationOrders_PatientId",
                table: "MedicationOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationOrders_PsyProblemListId",
                table: "MedicationOrders",
                column: "PsyProblemListId");

            migrationBuilder.CreateIndex(
                name: "IX_MentalStatusExaminations_InitialAssessmentFormId",
                table: "MentalStatusExaminations",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousItems_PatientId",
                table: "MiscellaneousItems",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_NewAppointments_ClinicalStaffID",
                table: "NewAppointments",
                column: "ClinicalStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_NewAppointments_PatientId",
                table: "NewAppointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalExams_InitialAssessmentFormId",
                table: "PhysicalExams",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProblemLists_InitialAssessmentFormId",
                table: "ProblemLists",
                column: "InitialAssessmentFormId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressNotes_InterventionId",
                table: "ProgressNotes",
                column: "InterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressNotes_PatientId",
                table: "ProgressNotes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PsychiatricAssessments_PatientId",
                table: "PsychiatricAssessments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PsyDiagnosisLists_PsychiatricAssessmentId",
                table: "PsyDiagnosisLists",
                column: "PsychiatricAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PsyProblemLists_PsychiatricAssessmentId",
                table: "PsyProblemLists",
                column: "PsychiatricAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_InitialAssessmentFormId",
                table: "Recommendations",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubstanceUseEntries_DiagnosisId",
                table: "SubstanceUseEntries",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_SurgicalHistories_InitialAssessmentFormId",
                table: "SurgicalHistories",
                column: "InitialAssessmentFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClinicalStaffID",
                table: "Users",
                column: "ClinicalStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PatientId",
                table: "Users",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministrationLogs");

            migrationBuilder.DropTable(
                name: "Availabilities");

            migrationBuilder.DropTable(
                name: "Billables");

            migrationBuilder.DropTable(
                name: "ClinicalStaffPatients");

            migrationBuilder.DropTable(
                name: "DischargedPatients");

            migrationBuilder.DropTable(
                name: "DrugUses");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "HistoryPresents");

            migrationBuilder.DropTable(
                name: "MedicalAllergies");

            migrationBuilder.DropTable(
                name: "MedicalHistories");

            migrationBuilder.DropTable(
                name: "MentalStatusExaminations");

            migrationBuilder.DropTable(
                name: "MiscellaneousItems");

            migrationBuilder.DropTable(
                name: "NewAppointments");

            migrationBuilder.DropTable(
                name: "PhysicalExams");

            migrationBuilder.DropTable(
                name: "ProgressNotes");

            migrationBuilder.DropTable(
                name: "PsyDiagnosisLists");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "SubstanceUseEntries");

            migrationBuilder.DropTable(
                name: "SurgicalHistories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MedicationOrders");

            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "IntakeForms");

            migrationBuilder.DropTable(
                name: "ProblemLists");

            migrationBuilder.DropTable(
                name: "Interventions");

            migrationBuilder.DropTable(
                name: "Diagnoses");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "ClinicalStaffs");

            migrationBuilder.DropTable(
                name: "PsyProblemLists");

            migrationBuilder.DropTable(
                name: "ServiceTypes");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "InitialAssessmentForms");

            migrationBuilder.DropTable(
                name: "PsychiatricAssessments");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
