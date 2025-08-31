namespace SafehavenPMS.ViewModel
{
    public class IntakeViewModel
    {
        public int IntakeId { get; set; }
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
    }
}