using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
    public class NewAppointment
    {
        [Key]
        public int ScheduleId { get; set; }
        public int PatientId { get; set; }
        public int? ClinicalStaffID { get; set; }
        public string Type { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string? ScheduleTime { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        public virtual Patient? Patient { get; set; } 
        public virtual ClinicalStaff? ClinicalStaff { get; set; }
    }
}

