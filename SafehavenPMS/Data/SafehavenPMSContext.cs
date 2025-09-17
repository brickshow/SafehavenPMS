using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using SafehavenPMS.Models;

namespace SafehavenPMS.Data
{
    public class SafehavenPMSContext : DbContext
    {
        public SafehavenPMSContext(DbContextOptions<SafehavenPMSContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<ClinicalStaff> ClinicalStaffs { get; set; }
        public DbSet<ClinicalStaffPatient> ClinicalStaffPatients { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<NewAppointment> NewAppointments { get; set; }
        public DbSet<MedicationOrder> MedicationOrders { get; set; }
        public DbSet<Admission> Admissions { get; set; }

        public DbSet<AdministrationLog> AdministrationLogs { get; set; }

        //Intake forms Entities
        public DbSet<IntakeForm> IntakeForms { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }

        //Assessment forms Entities
        public DbSet<InitialAssessmentForm> InitialAssessmentForms { get; set; }
        public DbSet<HistoryPresent> HistoryPresents { get; set; }
        public DbSet<DrugUse> DrugUses { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<MedicalAllergy> MedicalAllergies { get; set; }
        public DbSet<SurgicalHistory> SurgicalHistories { get; set; }
        public DbSet<PhysicalExam> PhysicalExams { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<SubstanceUseEntry> SubstanceUseEntries { get; set; }
        public DbSet<ProblemList> ProblemLists { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<MentalStatusExamination> MentalStatusExaminations { get; set; }

        //Psychiatric Assessment Entity
        public DbSet<PsychiatricAssessment> PsychiatricAssessments { get; set; }
        public DbSet<PsyProblemList> PsyProblemLists { get; set; }
        public DbSet<PsyDiagnosisList> PsyDiagnosisLists { get; set; }

        // Goals Entity
        public DbSet<Goal> Goals { get; set; }
        public DbSet<DischargedPatient> DischargedPatients { get; set; }

        // Add this for ServiceType
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Service> Services { get; set; }

        public DbSet<Intervention> Interventions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Many-to-many: Patient ↔ ClinicalStaff
            modelBuilder.Entity<ClinicalStaffPatient>()
                .HasKey(cp => new { cp.PatientId, cp.ClinicalStaffId });

            modelBuilder.Entity<ClinicalStaffPatient>()
                .HasOne(cp => cp.Patient)
                .WithMany(p => p.ClinicalStaffPatients)
                .HasForeignKey(cp => cp.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClinicalStaffPatient>()
                .HasOne(cp => cp.ClinicalStaff)
                .WithMany(c => c.ClinicalStaffPatients)
                .HasForeignKey(cp => cp.ClinicalStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // ClinicalStaff → Availability (Cascade is fine here)
            modelBuilder.Entity<ClinicalStaff>()
                .HasMany(c => c.Availabilities)
                .WithOne(a => a.ClinicalStaff)
                .HasForeignKey(a => a.ClinicalStaffID)
                .OnDelete(DeleteBehavior.Cascade);


            // Patient → MedicationOrders
            modelBuilder.Entity<MedicationOrder>()
                .HasOne(p => p.Patient)
                .WithMany(m => m.MedicationOrders)
                .HasForeignKey(k => k.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Medicine → MedicationOrders
            modelBuilder.Entity<MedicationOrder>()
                .HasOne(mo => mo.Medicine)
                .WithMany(m => m.MedicationOrders)
                .HasForeignKey(mo => mo.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicationAdministration → Patient
            modelBuilder.Entity<AdministrationLog>()
                .HasOne(ma => ma.Patient)
                .WithMany(p => p.AdministrationLogs) // Add a collection in Patient if not yet
                .HasForeignKey(ma => ma.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicationAdministration → Medicine
            // AdministrationLog → Medicine
            modelBuilder.Entity<AdministrationLog>()
                .HasOne(ma => ma.Medication)
                .WithMany(m => m.AdministrationLogs)
                .HasForeignKey(ma => ma.MedicationOrderId)  // <-- Use the FK property
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------
            // New Admission Relationships
            // ----------------------
            modelBuilder.Entity<Admission>()
              .HasOne(a => a.Patient)       // specify navigation property
              .WithMany()                    // if Patient has a collection of Admissions, put it here
              .HasForeignKey(a => a.PatientId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FamilyMember>()
                .HasOne<IntakeForm>()
                .WithMany(i => i.FamilyMembers)
                .HasForeignKey(f => f.IntakeFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure InitialAssessmentForm relationships
            modelBuilder.Entity<InitialAssessmentForm>()
                .HasOne(i => i.Patient)
                .WithOne()
                .HasForeignKey<InitialAssessmentForm>(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InitialAssessmentForm>()
                .HasOne(i => i.HistoryPresent)
                .WithOne(h => h.InitialAssessmentForm)
                .HasForeignKey<HistoryPresent>(h => h.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure DrugUse relationships
            modelBuilder.Entity<DrugUse>()
                .HasOne(d => d.InitialAssessmentForm)
                .WithMany(i => i.DrugUses)
                .HasForeignKey(d => d.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MedicalHistory relationship
            modelBuilder.Entity<MedicalHistory>()
                .HasOne(m => m.InitialAssessmentForm)
                .WithOne(i => i.MedicalHistory)
                .HasForeignKey<MedicalHistory>(m => m.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MedicalAllergy relationship
            modelBuilder.Entity<MedicalAllergy>()
                .HasOne(m => m.InitialAssessmentForm)
                .WithMany(i => i.MedicalAllergies)
                .HasForeignKey(m => m.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SurgicalHistory relationship
            modelBuilder.Entity<SurgicalHistory>()
                .HasOne(s => s.InitialAssessmentForm)
                .WithMany(i => i.SurgicalHistories)
                .HasForeignKey(s => s.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PhysicalExam relationship
            modelBuilder.Entity<PhysicalExam>()
                .HasOne(p => p.InitialAssessmentForm)
                .WithOne(i => i.PhysicalExam)
                .HasForeignKey<PhysicalExam>(p => p.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Diagnosis relationship
            modelBuilder.Entity<Diagnosis>()
                .HasOne(d => d.InitialAssessmentForm)
                .WithOne(i => i.Diagnosis)
                .HasForeignKey<Diagnosis>(d => d.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SubstanceUseEntry relationship
            modelBuilder.Entity<SubstanceUseEntry>()
                .HasOne(s => s.Diagnosis)
                .WithMany(d => d.SubstanceUseEntries)
                .HasForeignKey(s => s.DiagnosisId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProblemList relationship
            modelBuilder.Entity<ProblemList>()
                .HasOne(p => p.InitialAssessmentForm)
                .WithMany(i => i.Problems)
                .HasForeignKey(p => p.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.InitialAssessmentForm)
                .WithOne(i => i.Recommendation)
                .HasForeignKey<Recommendation>(r => r.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MentalStatusExamination relationship
            modelBuilder.Entity<MentalStatusExamination>()
                .HasOne(m => m.InitialAssessmentForm)
                .WithOne(i => i.MentalStatusExamination)
                .HasForeignKey<MentalStatusExamination>(m => m.InitialAssessmentFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // PsychiatricAssessment → Patient
            modelBuilder.Entity<PsychiatricAssessment>()
                .HasOne(pa => pa.Patient)
                .WithMany(p => p.PsychiatricAssessments) // Add a collection in Patient if not yet
                .HasForeignKey(pa => pa.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProblemList → Goals (One-to-Many)
            modelBuilder.Entity<Goal>()
                .HasOne(g => g.ProblemList)
                .WithMany(pl => pl.Goals)
                .HasForeignKey(g => g.PsyProblemListId)
                .OnDelete(DeleteBehavior.Cascade);

            // DischargedPatient → Patient (FK relationship)
            modelBuilder.Entity<DischargedPatient>()
                .HasOne(d => d.Patient)
                .WithMany()
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceType (1) <-> (Many) Service
            modelBuilder.Entity<Service>()
                .HasOne(s => s.ServiceType)
                .WithMany()
                .HasForeignKey(s => s.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Update IntakeForm → Patient relationship
            modelBuilder.Entity<IntakeForm>()
                .HasOne(i => i.Patient)
                .WithOne(p => p.IntakeForm)
                .HasForeignKey<IntakeForm>(i => i.PatientId)
                .IsRequired(false)  // Make the relationship optional
                .OnDelete(DeleteBehavior.Restrict);

            // Intervention → Patient (many-to-one)
            modelBuilder.Entity<Intervention>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Interventions)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Intervention → PsyProblemList (many-to-one)
            modelBuilder.Entity<Intervention>()
                .HasOne(i => i.Problem)
                .WithMany(pl => pl.Interventions)
                .HasForeignKey(i => i.PsyProblemListId)
                .OnDelete(DeleteBehavior.Restrict);

            // Intervention → ServiceType (many-to-one)
            modelBuilder.Entity<Intervention>()
                .HasOne(i => i.ServiceType)
                .WithMany()
                .HasForeignKey(i => i.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Intervention → Service (many-to-one)
            modelBuilder.Entity<Intervention>()
                .HasOne(i => i.ServiceModality)
                .WithMany()
                .HasForeignKey(i => i.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
