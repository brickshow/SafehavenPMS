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
                    ClinicalStaffID = table.Column<int>(type: "int", nullable: true),
                    FamilyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FamilyRelationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FamilyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FamilyEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActivatePortal = table.Column<bool>(type: "bit", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrentFacility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissions", x => x.AdmissionId);
                    table.ForeignKey(
                        name: "FK_Admissions_ClinicalStaffs_ClinicalStaffID",
                        column: x => x.ClinicalStaffID,
                        principalTable: "ClinicalStaffs",
                        principalColumn: "ClinicalStaffID");
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
                    ReferredBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Affiliation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationOrders",
                columns: table => new
                {
                    MedicationOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
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
                name: "PatientTransfers",
                columns: table => new
                {
                    TransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    FromFacility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToFacility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTransfers", x => x.TransferId);
                    table.ForeignKey(
                        name: "FK_PatientTransfers_Patients_PatientId",
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
                name: "IX_Admissions_ClinicalStaffID",
                table: "Admissions",
                column: "ClinicalStaffID");

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
                name: "IX_MentalStatusExaminations_InitialAssessmentFormId",
                table: "MentalStatusExaminations",
                column: "InitialAssessmentFormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewAppointments_ClinicalStaffID",
                table: "NewAppointments",
                column: "ClinicalStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_NewAppointments_PatientId",
                table: "NewAppointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTransfers_PatientId",
                table: "PatientTransfers",
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
                name: "IX_PsychiatricAssessments_PatientId",
                table: "PsychiatricAssessments",
                column: "PatientId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministrationLogs");

            migrationBuilder.DropTable(
                name: "Availabilities");

            migrationBuilder.DropTable(
                name: "ClinicalStaffPatients");

            migrationBuilder.DropTable(
                name: "DrugUses");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "HistoryPresents");

            migrationBuilder.DropTable(
                name: "MedicalAllergies");

            migrationBuilder.DropTable(
                name: "MedicalHistories");

            migrationBuilder.DropTable(
                name: "MentalStatusExaminations");

            migrationBuilder.DropTable(
                name: "NewAppointments");

            migrationBuilder.DropTable(
                name: "PatientTransfers");

            migrationBuilder.DropTable(
                name: "PhysicalExams");

            migrationBuilder.DropTable(
                name: "ProblemLists");

            migrationBuilder.DropTable(
                name: "PsychiatricAssessments");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "SubstanceUseEntries");

            migrationBuilder.DropTable(
                name: "SurgicalHistories");

            migrationBuilder.DropTable(
                name: "MedicationOrders");

            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "IntakeForms");

            migrationBuilder.DropTable(
                name: "Diagnoses");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "ClinicalStaffs");

            migrationBuilder.DropTable(
                name: "InitialAssessmentForms");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
