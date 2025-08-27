using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class AdministrationLog
    {
        [Key]
        public int AdministrationId { get; set; }

        // Foreign keys
        public int PatientId { get; set; }
        public int MedicationOrderId { get; set; }

        // Navigation properties (optional)
        public Patient Patient { get; set; }
        public MedicationOrder? Medication { get; set; }

        // Daily administration details
        public DateTime AdministrationDate { get; set; }
        public bool BreakfastTaken { get; set; } = false;
        public bool LunchTaken { get; set; } = false;
        public bool DinnerTaken { get; set; } = false;
        public bool BedtimeTaken { get; set; } = false;

        // Optional notes and audit info
        public string? Notes { get; set; }
        public int RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
