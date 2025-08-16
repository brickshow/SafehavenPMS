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
        public string? Description { get; set; } //Optional

        [Required(ErrorMessage ="Please provide visit type!")]
        public string VisitType { get; set; }
        public string? status { get; set; } = Enum.AppointmentEnum.Pending.ToString();
        public int AvailabilityId { get; set; }
        public int TimeSlotId { get; set; }
    }
}
