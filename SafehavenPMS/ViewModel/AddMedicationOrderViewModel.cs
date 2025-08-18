using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel
{
    public class AddMedicationOrderViewModel
    {
        public List<ClinicalStaffPatient> Patients { get; set; }
        public List<Medicine> Medicines { get; set; }

        public int? SelectedMedicineId { get; set; }
        public string Form { get; set; }
        public string Unit { get; set; }

    }
}
