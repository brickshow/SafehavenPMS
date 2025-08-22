using SafehavenPMS.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace SafehavenPMS.Models
{
    // Main Availability Entry
    public class Availability
    {
        public int AvailabilityId { get; set; }

        // Link to the Doctor
        public int ClinicalStaffID { get; set; }
        public ClinicalStaff ClinicalStaff { get; set; }

        // Day of the week (Mon-Sun)
        public DayOfWeek Day { get; set; }
        public DateTime? SlotDate { get; set; }

        // Start and End time for slot
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Notes { get; set; }

        // Availability status
        public string Status { get; set; } = AvailabilityStatus.Available.ToString();
    }
}