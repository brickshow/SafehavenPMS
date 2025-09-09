using SafehavenPMS.Models;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AdmitPatientViewModel
    {
        // ----------------------
        // Patient Details
        // ----------------------
        public int? AdmissionId { get; set; } // Make nullable to handle no admission case

        public int PatientId { get; set; }
        public string? FullName { get; set; }
        public string? Sex { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }
        public string? Age { get; set; }
        public string? EducationalAttainment { get; set; }
        public string? Occupation { get; set; }
        public string? Religion { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // ----------------------
        // Clinical Staff
        // ----------------------
        public string? PhysicianName { get; set; }

        public int? PhysicianId { get; set; }

        public int? PsychiatristId { get; set; }
        public int? PsychologistId { get; set; }
        public int? PsychometricianId { get; set; }
        public int? SocialWorkerId { get; set; }
        public int? RecoveryCoachId { get; set; }

        // ----------------------
        // Family / Payer Info
        // ----------------------
        [MaxLength(150, ErrorMessage = "Family Name cannot exceed 150 characters.")]
        public string? FamilyName { get; set; }

        [MaxLength(100, ErrorMessage = "Relationship cannot exceed 100 characters.")]
        public string? FamilyRelationship { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string? FamilyPhone { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? FamilyEmail { get; set; }

        public bool ActivatePortal { get; set; }

        // ----------------------
        // Admission Tracking
        // ----------------------
        public DateTime AdmissionDate { get; set; } = DateTime.Now;

        // Add this property for drug dependency status
        public bool IsDrugDependent { get; set; }

        // Diagnosis and Recommendation
        public string? Diagnosis { get; set; }
        public string? Recommendation { get; set; }

        public DateTime? CompletedDate { get; set; }

        // ----------------------
        // Audit
        // ----------------------
        [MaxLength(50, ErrorMessage = "Created By cannot exceed 50 characters.")]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(50, ErrorMessage = "Updated By cannot exceed 50 characters.")]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? Status { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EndedBy { get; set; }


        // ----------------------
        // Datalist
        // ----------------------
        public List<Patient> PatientMatches { get; set; } = new List<Patient>();
        public string? ReceivingFacility { get; set; }

        public string? ProgramType { get; set; }
        public string? Reason { get; set; }
    }
}
