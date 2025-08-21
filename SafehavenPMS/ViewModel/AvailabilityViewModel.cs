using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AvailabilityViewModel
    {
        public int Id { get; set; }

        // Link to the Doctor
        public int ClinicalStaffID { get; set; }
        public ClinicalStaff ClinicalStaff { get; set; }

        // Day of the week (Mon-Sun)
        public DayOfWeek Day { get; set; }

        // Start and End time for slot
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Availability status
        public AvailabilityStatus Status { get; set; } = AvailabilityStatus.Available;
    }
}
