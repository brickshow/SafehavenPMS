using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class AppointmentPageViewModel
    {
        public List<NewAppointment> Appointments { get; set; }          // for parent
        public List<AppointmentPendingApprovalViewModel> PendingAppointments { get; set; } // for child
        public List<AddNewPatientViewModel> WaitlistedPatients { get; set; }
    }
}

