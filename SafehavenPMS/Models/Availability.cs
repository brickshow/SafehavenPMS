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

        //Clinical staff foreign key
        public int ClinicalStaffId { get; set; }
        public ClinicalStaff ClinicalStaff { get; set; }
        // Navigation property
        public ICollection<AvailabilityDay> Days { get; set; }
    }

    // Days within the availability date range
    public class AvailabilityDay
    {
        [Key]
        public int DayId { get; set; }

        [Required]
        public string DayName { get; set; } // e.g., "Monday", "Tuesday"

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

        // Foreign key
        public int DayId { get; set; }

        // Navigation
        [ForeignKey("DayId")]
        public AvailabilityDay Day { get; set; }
    }
}
