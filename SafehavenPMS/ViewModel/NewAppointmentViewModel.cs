using SafehavenPMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SafehavenPMS.ViewModel
{
    public class NewAppointmentViewModel
    {
        public int? PatientId { get; set; }
        public string PatientName { get; set; }

        public int? ClinicalStaffID { get; set; }
        public string ClinicalStaffName { get; set; }

        public int? AvailabilityId { get; set; }
        public int? TimeSlotId { get; set; }

        public string VisitType { get; set; }
        public string Description { get; set; }

        public DateTime? SelectedDate { get; set; }

        // Lists for dropdowns
        public List<Patient> Patients { get; set; }
        public List<ClinicalStaff> ClinicalStaffs { get; set; }
        public List<Availability> Availabilities { get; set; }
    }
}
