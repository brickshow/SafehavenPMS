using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel
{
    public class TransferViewModel
    {
        public int? TransferId { get; set; }
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? Photo { get; set; }
        public string? FromFacility { get; set; }
        public string? ToFacility { get; set; }
        public string? ProgramType { get; set; }
        public string? Reason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; }
        public DateTime? TransferDate { get; set; }
    }
}