using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    public class Admission
    {
        [Key]
        public int AdmissionId { get; set; }

        // Link to Patient
        [Required]
        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; } // navigation

        // Staff assignments
        public int? PhysicianId { get; set; }
        public int? PsychiatristId { get; set; }
        public int? PsychologistId { get; set; }
        public int? PsychometricianId { get; set; }
        public int? SocialWorkerId { get; set; }
        public int? RecoveryCoachId { get; set; }

        // Family / Payer info
        [MaxLength(150)]
        public string? FamilyName { get; set; }

        [MaxLength(100)]
        public string? FamilyRelationship { get; set; }

        [MaxLength(20)]
        public string? FamilyPhone { get; set; }

        [MaxLength(100)]
        public string? FamilyEmail { get; set; }

        public bool ActivatePortal { get; set; }

        // Admission tracking
        public DateTime AdmissionDate { get; set; } = DateTime.Now;

        // Audit
        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Endedby { get; set; }
        public string? status { get; set; }

        public bool IsDrugDependent { get; set; }
        public string? Diagnosis { get; set; }
        public string? Recommendation { get; set; }
    }
}
