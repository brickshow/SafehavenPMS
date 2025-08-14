using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel
{
    public class ClinicalStaffProfileViewModel
    {
        public List<ClinicalStaff> Staffs { get; set; }
        public List<Patient> Patients { get; set; }
        public List<AvailabilityViewModel> Availability { get; set; } = new();


        // NEW: separate model for adding availability
        public AvailabilityViewModel NewAvailability { get; set; } = new AvailabilityViewModel();
    }
}
