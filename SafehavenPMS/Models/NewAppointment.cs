using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class NewAppointment
    {
        [Key]
        public int AppointmentID { get; set; }
        public int ClinicalStaffID { get; set; }//Foreign key
        public ClinicalStaff? ClinicalStaff { get; set; }//Navigation
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string VisitType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime BookedAt { get; set; }

    }
}
