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
            modelBuilder.Entity<Patient>()
                 .HasKey(p => new { p.PatientId, p.ClinicalStaffID });
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
