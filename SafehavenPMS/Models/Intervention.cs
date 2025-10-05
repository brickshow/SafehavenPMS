using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
    public class    Intervention
    {
        [Key]
        public int InterventionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int PsyProblemListId { get; set; }

        public int? ServiceTypeId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [StringLength(100)]
        public string DurationFrequency { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public string NotedBy { get; set; }

        public DateTime? DateAdded { get; set; }
        public DateTime? StartDate { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [ForeignKey("PsyProblemListId")]
        public PsyProblemList Problem { get; set; }

        [ForeignKey("ServiceTypeId")]
        public ServiceType ServiceType { get; set; }

        [ForeignKey("ServiceId")]
        public Service ServiceModality { get; set; }
    }
}
