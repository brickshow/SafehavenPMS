namespace SafehavenPMS.ViewModel
{
    public class IntakeViewModel
    {
        public int IntakeId { get; set; }
        public string? FullName { get; set; }
        public string? ReferredBy { get; set; }
        public string? ReferredByPhoneNumber { get; set; }
        public string? IntakeOfficer { get; set; }
        public DateTime? IntakeDate { get; set; }
        public string? CompletedDate { get; set; }
        public string? IntakeStatus { get; set; }
    }
}
