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
        public DbSet<AvailabilityDay> AvailabilityDays { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Medicine> Medicines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the many-to-many relationship
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

            // ClinicalStaff → Availability
            modelBuilder.Entity<ClinicalStaff>()
                .HasMany(c => c.Availabilities)
                .WithOne(a => a.ClinicalStaff)
                .HasForeignKey(a => a.ClinicalStaffID)
                .OnDelete(DeleteBehavior.Cascade);

            // Availability → AvailabilityDay
            modelBuilder.Entity<Availability>()
                .HasMany(a => a.Days)
                .WithOne(d => d.Availability)
                .HasForeignKey(d => d.AvailabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // AvailabilityDay → TimeSlot
            modelBuilder.Entity<AvailabilityDay>()
                .HasMany(d => d.TimeSlots)
                .WithOne(ts => ts.Day)
                .HasForeignKey(ts => ts.DayId)
                .OnDelete(DeleteBehavior.Cascade);

            // Availability -> Appointment
            modelBuilder.Entity<Availability>()
                .HasMany(a => a.Appointments)
                .WithOne(ap => ap.Availability)
                .HasForeignKey(ap => ap.AvailabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient -> Appointment
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // ClinicalStaff -> Appointment
            modelBuilder.Entity<ClinicalStaff>()
                .HasMany(s => s.Appointments)
                .WithOne(a => a.Staff)
                .HasForeignKey(a => a.ClinicalStaffID)
                .OnDelete(DeleteBehavior.Cascade);


            // Global safeguard: Disable all cascade deletes
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
