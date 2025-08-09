using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AvailabilityViewModel
    {
        // From Availability
        public int AvailabilityId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // From AvailabilityDay
        public int DayId { get; set; }

        [Required(ErrorMessage = "Day name is required.")]
        public string DayName { get; set; }

        // From TimeSlot
        public int TimeSlotId { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }
}
