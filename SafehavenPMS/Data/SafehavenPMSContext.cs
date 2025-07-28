using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Models;

namespace SafehavenPMS.Data
{
    public class SafehavenPMSContext : DbContext
    {
        public SafehavenPMSContext(DbContextOptions<SafehavenPMSContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Patient> Patients { get; set; }
        public DbSet<Models.Address> Addresses { get; set; }
        public DbSet<Models.ClinicalStaff> ClinicalStaffs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ////Relationship between Patient and assigned staff
            /////Many to many relation
            modelBuilder.Entity<ClinicalStaffPatient>()
                .HasKey(cs => new { cs.PatientId, cs.ClinicalStaffId });

            modelBuilder.Entity<ClinicalStaffPatient>()
                .HasOne(cs => cs.Patient)
                .WithMany(p => p.ClinicalStaffPatients)
                .HasForeignKey(cs => cs.PatientId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade loop

            modelBuilder.Entity<ClinicalStaffPatient>()
                .HasOne(cs => cs.ClinicalStaff)
                .WithMany(staff => staff.ClinicalStaffPatients)
                .HasForeignKey(cs => cs.ClinicalStaffId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascade loop


            //Address relationship
            modelBuilder.Entity<Models.Patient>()
                .HasOne(p => p.Address)
                .WithMany(a => a.Patients)
                .HasForeignKey(p => p.AddressID);

            //Address relationship for Clinical Staff
            modelBuilder.Entity<Models.ClinicalStaff>()
                .HasOne(cs => cs.Address)
                .WithMany(a => a.ClinicalStaffs)
                .HasForeignKey(cs => cs.AddressID);
        }
    }
}
