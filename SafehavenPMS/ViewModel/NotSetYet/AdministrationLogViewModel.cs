using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class AdministrationLogViewModel
    {
        public int AdministrationLogId { get; set; }
        public int MedicationOrderId { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? AdministrationDate { get; set; }
        public int TotalMeds { get; set; }
        public string? ScheduleTimes { get; set; }
        public string? AdministrationStatus { get; set; }

                // Daily administration details
        public bool BreakfastTaken { get; set; } = false;
        public bool LunchTaken { get; set; } = false;
        public bool DinnerTaken { get; set; } = false;
        public bool BedtimeTaken { get; set; } = false;

        // List of medications for this patient
        public List<MedicationOrderViewModel>? Medications { get; set; }
    }
}

