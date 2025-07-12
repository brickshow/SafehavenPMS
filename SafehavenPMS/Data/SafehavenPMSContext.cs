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
        public DbSet<Models.Religion> Religions { get; set; }
        public DbSet<Models.Nationality> Nationalities { get; set; }

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

            //Religion relationship
            modelBuilder.Entity<Models.Patient>()
                .HasOne(e => e.Religion)
                .WithMany(p => p.Patients)
                .HasForeignKey(p => p.ReligionID);

            //Nationality relationship
            modelBuilder.Entity<Models.Patient>()
                .HasOne(p => p.Nationality)
                .WithMany(n => n.Patients)
                .HasForeignKey(p => p.NationalityID);

            //For Patient Case
            modelBuilder.Entity<Models.PatientCase>()
                .HasOne(pc => pc.Patient)
                .WithMany(p => p.PatientCases)
                .HasForeignKey(pc => pc.PatientId);

            //Address relationship
            modelBuilder.Entity<Models.Patient>()
                .HasOne(p => p.Address)
                .WithMany(a => a.Patients)
                .HasForeignKey(p => p.AddressID);

            //Data seeding for Education Levels
            modelBuilder.Entity<Models.EducationLevel>().HasData(
                new Models.EducationLevel { EducationLevelId = 1, EducationLevelName = "Primary" },
                new Models.EducationLevel { EducationLevelId = 2, EducationLevelName = "Secondary" },
                new Models.EducationLevel { EducationLevelId = 3, EducationLevelName = "Tertiary" },
                new Models.EducationLevel { EducationLevelId = 4, EducationLevelName = "Postgraduate" }
            );

            //Data seeding for Marital Statuses
            modelBuilder.Entity<Models.MaritalStatus>().HasData(
                new Models.MaritalStatus { MaritalStatusId = 1, MaritalStatusType = "Single" },
                new Models.MaritalStatus { MaritalStatusId = 2, MaritalStatusType = "Married" },
                new Models.MaritalStatus { MaritalStatusId = 3, MaritalStatusType = "Divorced" },
                new Models.MaritalStatus { MaritalStatusId = 4, MaritalStatusType = "Widowed" }
            );
        }

        //Data Seeding

    }
}
