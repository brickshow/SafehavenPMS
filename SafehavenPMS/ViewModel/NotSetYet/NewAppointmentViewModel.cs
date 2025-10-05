using Microsoft.AspNetCore.Mvc.Rendering;
using SafehavenPMS.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class NewAppointmentViewModel
    {
        public int AppointmentID { get; set; }
        [Required(ErrorMessage = "Please add Patient")]
        public int PatientId { get; set; }
        public string? PatientFullname { get; set; }

        [Required(ErrorMessage = "Please assign a Doctor")]
        public int ClinicalStaffID { get; set; }//Foreign key
        public DayOfWeek Day { get; set; }

        [Required(ErrorMessage = "Time slot is required")]
        public TimeSpan TimeSlot { get; set; }
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Visit type is required")]
        public string VisitType { get; set; }
        public string? Description { get; set; }

        public DateTime SelectedDate { get; set; }

        // Lists for dropdowns
        public List<Patient>? Patients { get; set; }
        public List<ClinicalStaff>? ClinicalStaffs { get; set; }
        public List<Availability>? Availabilities { get; set; }
    }
}

