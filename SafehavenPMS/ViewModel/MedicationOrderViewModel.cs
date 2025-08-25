using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class MedicationOrderViewModel
    {
        public int MedicationOrderId { get; set; }

        // Patient
        [Required(ErrorMessage = "Please select a patient")]
        public int PatientId { get; set; }

        [Display(Name = "Patient Name")]
        public string? PatientName { get; set; }

        // Medicine
        [Required(ErrorMessage = "Please select a medicine")]
        public int MedicineId { get; set; }

        [Display(Name = "Medicine Name")]
        public string? MedicineName { get; set; }

        // Dosage
        [Required(ErrorMessage = "Unit per dose is required")]
        [Range(1, 1000, ErrorMessage = "Unit per dose must be at least 1")]
        [Display(Name = "Unit per dose")]
        public int UnitPerDose { get; set; }

        public string? UnitPerDoseDisplay { get; set; }

        [StringLength(250, ErrorMessage = "Note cannot exceed 250 characters")]
        public string? Note { get; set; }

        // Schedule
        [Required(ErrorMessage = "Scheduled type is required")]
        [Display(Name = "Scheduled Type")]
        public string ScheduledType { get; set; } = "Daily"; // Daily / NonDaily
        public string? ScheduleTimes { get; set; }

        [Range(1, 365, ErrorMessage = "Days interval must be at least 1")]
        [Display(Name = "Days Interval")]
        public int? DaysInterval { get; set; } // Only required if NonDaily

        public bool Breakfast { get; set; } = true;
        public bool Lunch { get; set; } = true;
        public bool Dinner { get; set; } = true;
        public bool Bedtime { get; set; } = false;

        // Dates
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Discontinue Date")]
        public DateTime? DiscontinueDate { get; set; }

        public string? CreatedBy { get; set; }

        public string? Status { get; set; }

        public bool NoDiscontinueDate { get; set; } = false;

        // Dropdowns for Razor
        public IEnumerable<SelectListItem>? PatientList { get; set; }
        public IEnumerable<SelectListItem>? MedicineList { get; set; }
    }
}
