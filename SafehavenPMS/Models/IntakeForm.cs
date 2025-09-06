using SafehavenPMS.ViewModel;
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

        [Required(ErrorMessage = "This field is required")]
        public string ReferredBy { get; set; } = string.Empty;
        public string? Affiliation { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Audit fields
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "System";
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Completed tracking
        public DateTime? CompletedAt { get; set; }

        // Presenting problem / intake details
        public string? ProblemPresentation { get; set; }
        public string? CouncilorImpression { get; set; }
        public string? OtherFamilyDetails { get; set; }

        // Family constellation
        public List<FamilyMember> FamilyMembers { get; set; } = new();

        // Problems and impressions
        public string? PresentingComplaint { get; set; }
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
}
