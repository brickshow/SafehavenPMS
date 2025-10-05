using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class SchedulingViewModel
    {
        public int ScheduleId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public int? ClinicalStaffID { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        public DateTime? ScheduleDate { get; set; }

        [StringLength(20)]
        public string? ScheduleTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Optionally, you can add Patient and ClinicalStaff display properties if needed
        public string? PatientName { get; set; }
        public string? ClinicalStaffName { get; set; }
    }
}   
