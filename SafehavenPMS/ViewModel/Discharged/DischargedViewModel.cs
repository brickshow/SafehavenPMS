using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel
{
    public class DischargedViewModel
    {
        public int? DischargeId { get; set; }
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? Photo { get; set; }
        public string? Reason { get; set; }
        public string? DischargedBy { get; set; }
        public DateTime? DischargedAt { get; set; }
        public string? Status { get; set; }
        public DateTime? DischargeDate { get; set; }
    }
}