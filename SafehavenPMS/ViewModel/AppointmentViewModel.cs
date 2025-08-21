using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AppointmentViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ClinicalStaffID { get; set; }

        public string? PatientName { get; set; }
        public string? ClinicalStaffName { get; set; }
        public string? Description { get; set; } // Optional

        [Required(ErrorMessage = "Please provide visit type!")]
        public string VisitType { get; set; }

        public string? Status { get; set; } = Enum.AppointmentEnum.Pending.ToString();

        [Required]
        public int AvailabilityId { get; set; } // Booked slot

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public DateTime SelectedDate { get; set; }
    }
}
