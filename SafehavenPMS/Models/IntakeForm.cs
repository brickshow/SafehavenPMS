using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    // Main class for storing patient intake information and related data
    public class IntakeForm
    {
        // Unique identifier for the intake form
        public int Id { get; set; }
        // Date and time when the form was created
        public DateTime CreatedAt { get; set; }
        // Staff member who created the form
        public string CreatedBy { get; set; }
        // Collection of patient's family members
        public List<FamilyMember> FamilyMembers { get; set; } = new();
        // Additional notes about family situation
        public string? OtherFamilyDetails { get; set; }
        // List of patient's reported problems
        public List<PresentingProblem> PresentingProblems { get; set; } = new();
        // Counselor's notes and observations
        public List<CounselorImpression> CounselorImpressions { get; set; } = new();
    }

    // Stores information about individual family members
    public class FamilyMember
    {
        // Unique identifier for the family member
        public int Id { get; set; }
        // Links to the parent intake form
        public int IntakeFormId { get; set; }
        // Full name of family member
        public string? Name { get; set; }
        // Age of family member (optional)
        public int? Age { get; set; }
        // Relationship to patient (e.g., "Mother", "Brother")
        public string? Relationship { get; set; }
        // Additional notes about the family member
        public string? Comments { get; set; }
    }

    // Records problems or issues reported by the patient
    public class PresentingProblem
    {
        // Unique identifier for the problem
        public int Id { get; set; }
        // Links to the parent intake form
        public int? IntakeFormId { get; set; }
        // Details of the reported problem
        public string? Description { get; set; }
    }

    // Stores counselor's assessment and observations
    public class CounselorImpression
    {
        // Unique identifier for the impression
        public int Id { get; set; }
        // Links to the parent intake form
        public int? IntakeFormId { get; set; }
        // Counselor's notes and diagnosis
        public string? Description { get; set; }
    }
}