using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class Scheduling
    {
        [Key]
        public int ScheduleId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string Type { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ScheduleTime { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}
