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
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ClinicalStaffPatient> ClinicalStaffPatients { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<AvailabilityDay> AvailabilityDays { get; set; }

        public DbSet<TimeSlot> TimeSlots { get; set; }

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

            // Configure Address relationships with explicit foreign key
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Address)
                .WithMany()
                .HasForeignKey(p => p.AddressID) // Explicitly map AddressID
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClinicalStaff>()
                .HasOne(c => c.Address)
                .WithMany()
                .HasForeignKey(c => c.AddressID) // Explicitly map AddressID
                .OnDelete(DeleteBehavior.Restrict);

            //Configure relationshio between entities for appoinment
            modelBuilder.Entity<TimeSlot>()
                .HasOne(t => t.Day)
                .WithMany(t => t.TimeSlots)
                .HasForeignKey(d => d.DayId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AvailabilityDay>()
                .HasOne(a => a.Availability)
                .WithMany(a => a.Days)
                .HasForeignKey(d => d.AvailabilityId)
                   .OnDelete(DeleteBehavior.Restrict);


            // Global safeguard: Disable all cascade deletes
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
