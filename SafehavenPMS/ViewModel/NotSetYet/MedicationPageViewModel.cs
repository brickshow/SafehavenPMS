using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class MedicationPageViewModel
    {
        public List<MedicineViewModel> Medicines { get; set; }
        public List<MedicationOrderViewModel> MedicationOrders { get; set; }
        public List<AdministrationLogViewModel> AdministrationLogs { get; set; } = new List<AdministrationLogViewModel>();
    }
}

