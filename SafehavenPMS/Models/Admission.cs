using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    public class Admission
    {
        [Key]
        public int AdmissionId { get; set; }

        // Case identifier stored as formatted string "CASE-000001"
        // changed from int? to string? to support formatted case id
        public string? CaseId { get; set; } // was int? previously

        public int? CaseIdInt => TryParseCaseId(CaseId);

        private int? TryParseCaseId(string? caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId)) return null;
            if (caseId.StartsWith("CASE-") && int.TryParse(caseId.Substring(5), out var n)) return n;
            return null;
        }

        // Link to Patient
        [Required]
        public int PatientId { get; set; }           // made non-nullable to reflect required patient
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }        // navigation

        // keep per-role FKs for the AdmitPatient view (backwards compatible)
        public int? PhysicianId { get; set; }
        [ForeignKey("PhysicianId")]
        public ClinicalStaff? Physician { get; set; }

        public int? PsychologistId { get; set; }
        [ForeignKey("PsychologistId")]
        public ClinicalStaff? Psychologist { get; set; }

        public int? PsychometricianId { get; set; }
        [ForeignKey("PsychometricianId")]
        public ClinicalStaff? Psychometrician { get; set; }

        public int? SocialWorkerId { get; set; }
        [ForeignKey("SocialWorkerId")]
        public ClinicalStaff? SocialWorker { get; set; }

        public int? RecoveryCoachId { get; set; }
        [ForeignKey("RecoveryCoachId")]
        public ClinicalStaff? RecoveryCoach { get; set; }

        // New: reuse the ClinicalStaffPatient join entity to list assignments for the patient
        // This allows using the existing ClinicalStaffPatient records without changing the AdmitPatient view
        public ICollection<ClinicalStaffPatient> ClinicalStaffPatients { get; set; } = new List<ClinicalStaffPatient>();

        // // Family / Payer info
        // [MaxLength(150)]
        // public string? FamilyName { get; set; }

        // [MaxLength(100)]
        // public string? FamilyRelationship { get; set; }

        // [MaxLength(20)]
        // public string? FamilyPhone { get; set; }

        // [MaxLength(100)]
        // public string? FamilyEmail { get; set; }

        // public bool ActivatePortal { get; set; } = true;

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

        [MaxLength(50)]
        public string? EndedBy { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }
        public string? ProgramType { get; set; }
    }
}
