using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel
{
    public class ClinicalStaffProfileViewModel
    {
        public List<ClinicalStaff> Staffs { get; set; }
        public List<Patient> Patients { get; set; }
        public List<Availability> Availability { get; set; }
    }
}
