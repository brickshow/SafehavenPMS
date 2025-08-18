using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    // Main Availability Entry
    public class Availability
    {
        [Key]
        public int AvailabilityId { get; set; }

        [Required]
        public string Title { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }//Not required

        public bool NoEndDate { get; set; } // Indicates if the availability has no end date

        //Clinical staff foreign key
        public int ClinicalStaffID { get; set; }
        public ClinicalStaff ClinicalStaff { get; set; }
        // Navigation property
        public ICollection<AvailabilityDay> Days { get; set; }

        //Navigaton property to Appointment
        public ICollection<Appointment> Appointments { get; set; } // Navigation property for appointments
    }

    // Days within the availability date range
    public class AvailabilityDay
    {
        [Key]
        public int DayId { get; set; }

        [Required]
        public string DayName { get; set; } // e.g., "Monday", "Tuesday"
        public bool IsAvailable { get; set; } = true;

        public DateTime Date { get; set; }

        // Foreign key
        public int AvailabilityId { get; set; }

        // Navigation
        [ForeignKey("AvailabilityId")]
        public Availability Availability { get; set; }

        public ICollection<TimeSlot> TimeSlots { get; set; }
    }

    // Time slots for a specific day
    public class TimeSlot
    {
        [Key]
        public int TimeSlotId { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Foreign key
        public int DayId { get; set; }

        // Navigation
        [ForeignKey("DayId")]
        public AvailabilityDay Day { get; set; }
    }
}
