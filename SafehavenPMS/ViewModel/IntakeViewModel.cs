namespace SafehavenPMS.ViewModel
{
    public class IntakeViewModel
    {
        public int IntakeId { get; set; }
        public int PatientId { get; set; }
        public string? FullName { get; set; }
        public string? Age { get; set; }
        public string? Sex { get; set; }
        public string? Address { get; set; }
        public string? ReferredBy { get; set; }
        public string? ReferredByPhoneNumber { get; set; }
        public string? IntakeOfficer { get; set; }
        public DateTime? IntakeDate { get; set; }
        public string? CompletedDate { get; set; }
        public DateTime? DateOfReferral { get; set; }
        public string? ReasonForIntake { get; set; }
        public string? Occupation { get; set; }
        public string? IntakeStatus { get; set; }

        //Presentinf problem
        public string? ProblemPresentation { get; set; }
        public string? CouncilorImpression { get; set; }
        public string? OtherFamilyDetails { get; set; } = string.Empty;

        // Family section
        public List<FamilyMemberVm> FamilyMembers { get; set; } = new();
        public FamilyMemberVm NewFamilyMember { get; set; } = new(); 

    }

    public class FamilyMemberVm
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Relationship { get; set; }
        public string? Comments { get; set; }

    }
}