using Microsoft.EntityFrameworkCore;

namespace SafehavenPMS.Data
{
    public class SafehavenPMSContext : DbContext
    {
        public SafehavenPMSContext(DbContextOptions<SafehavenPMSContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Patient> Patients { get; set; }
        public DbSet<Models.PatientCase> PatientCases { get; set; }
        public DbSet<Models.EducationLevel> EducationLevels { get; set; }

        public DbSet<Models.MaritalStatus> MaritalStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Configure entity properties, relationships, etc. here
            modelBuilder.Entity<Models.Patient>()
                .HasOne(p => p.MaritalStatus)
                .WithMany(m => m.Patients)
                .HasForeignKey(p => p.MaritalStatusID);

            //EducationLevel relationship
            modelBuilder.Entity<Models.Patient>()
                .HasOne(p => p.EducationLevel)
                .WithMany(e => e.Patients)
                .HasForeignKey(p => p.EducationLevelID);

            //For Patient Case
            modelBuilder.Entity<Models.PatientCase>()
                .HasOne(pc => pc.Patient)
                .WithMany(p => p.PatientCases)
                .HasForeignKey(pc => pc.PatientId);
        }
    }
}
