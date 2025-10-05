using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class ClinicalStaffProfileViewModel
    {
        public List<ClinicalStaff> Staffs { get; set; }
        public List<Patient> Patients { get; set; }
        public List<Availability> Availability { get; set; }

        public List<DayAvailabilityViewModel> Days { get; set; }
    }
}

