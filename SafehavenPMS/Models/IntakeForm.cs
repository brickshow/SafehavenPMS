using SafehavenPMS.ViewModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
     public class IntakeForm
    {
        // Primary key
        [Key]
        public int IntakeFormsId { get; set; }

        // Patient relationship
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public string? AccompaniedBy { get; set; }
        public string? Affiliation { get; set; }
        public string? PhoneNumber { get; set; }

        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "System";
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Completed tracking
        public DateTime? CompletedAt { get; set; }

        // Presenting problem / intake details
        public string? ProblemPresentation { get; set; }
        public string? CouncilorImpression { get; set; }
        public string? OtherFamilyDetails { get; set; }
        public string? Status { get; set; }

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

