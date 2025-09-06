using SafehavenPMS.Models;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AddMedicationOrderViewModel
    {
        // Dropdown sources
        public List<Patient> Patients { get; set; } = new List<Patient>();
        public List<Medicine> Medicines { get; set; } = new List<Medicine>();

        // Selected values
        [Required(ErrorMessage = "Please select a patient.")]
        public int? SelectedPatientId { get; set; }

        [Required(ErrorMessage = "Please select a medicine.")]
        public int? SelectedMedicineId { get; set; }

        // Medicine details (display only)
        public string Form { get; set; }
        public string Unit { get; set; }

        // Medication order fields
        [Required(ErrorMessage = "Dose is required.")]
        [Range(0.1, 9999, ErrorMessage = "Please enter a valid dose.")]
        public decimal Dose { get; set; }

        public string Instruction { get; set; }

        [Required(ErrorMessage = "Frequency is required.")]
        public string Frequency { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}
