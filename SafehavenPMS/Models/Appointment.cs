using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        // Foreign key to Availability
        [Required]
        public int AvailabilityId { get; set; }
        [ForeignKey("AvailabilityId")]
        public Availability? Availability { get; set; } // Navigation property


        // Foreign key to Staff
        [Required]
        public int ClinicalStaffID { get; set; }
        [ForeignKey("ClinicalStaffID")]
        public ClinicalStaff? Staff { get; set; } // Navigation property

        // Foreign key to Patient
        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; } // Navigation property


        public string VisitType { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = Enum.AppointmentEnum.Pending.ToString(); //Set the default value to Pending
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //TODO: Separate the patient case table
    }
}   
