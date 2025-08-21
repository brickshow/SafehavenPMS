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
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedicationOrder> MedicationOrders { get; set; }

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

            // Patient → Appointments
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // ClinicalStaff → Appointments
            modelBuilder.Entity<ClinicalStaff>()
                .HasMany(s => s.Appointments)
                .WithOne(a => a.Staff)
                .HasForeignKey(a => a.ClinicalStaffID)
                .OnDelete(DeleteBehavior.Restrict); // or .NoAction

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
        }

    }
}
