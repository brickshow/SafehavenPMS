using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class IntakeForm
    {
        // Primary key
        [Key]
        public int IntakeFormsId { get; set; }

        // Patient relationship
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        // Referral information
        [Required(ErrorMessage = "Date of referral is required")]
        public DateTime DateOfReferral { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string ReferredBy { get; set; } = string.Empty;
        public string? Affiliation { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Intake status and creation info
        public string? IntakeStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "System";

        // Family constellation
        public List<FamilyMember> FamilyMembers { get; set; } = new();
        public string? OtherFamilyDetails { get; set; }

        // Problems and impressions
        public string? PresentingComplaint { get; set; }
        public List<PresentingProblem> PresentingProblems { get; set; } = new();
        public List<CounselorImpression> CounselorImpressions { get; set; } = new();
    }

    public class FamilyMember
    {
        public int Id { get; set; }
        public int IntakeFormId { get; set; }
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Relationship { get; set; }
        public string? Comments { get; set; }

        // Navigation property
        public IntakeForm? PatientIntake { get; set; }
    }

    public class PresentingProblem
    {
        public int Id { get; set; }
        public int IntakeFormId { get; set; }
        public string? Description { get; set; }

        // Navigation property
        public IntakeForm? PatientIntake { get; set; }
    }

    public class CounselorImpression
    {
        public int Id { get; set; }
        public int IntakeFormId { get; set; }
        public string? Description { get; set; }

        // Navigation property
        public IntakeForm? PatientIntake { get; set; }
    }
}
