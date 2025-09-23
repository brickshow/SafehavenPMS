using System;

namespace SafehavenPMS.Models
{
    public class MiscellaneousItem
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}