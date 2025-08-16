using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        // Keep FK for reference
        [Required]
        public int AvailabilityId { get; set; }
        [ForeignKey("AvailabilityId")]
        public Availability? Availability { get; set; }

        [Required]
        public int ClinicalStaffID { get; set; }
        public ClinicalStaff? Staff { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public string VisitType { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = Enum.AppointmentEnum.Pending.ToString();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
