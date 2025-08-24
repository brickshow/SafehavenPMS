using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel
{
    public class AppointmentPageViewModel
    {
        public List<NewAppointment> Appointments { get; set; }          // for parent
        public List<AppointmentPendingApprovalViewModel> PendingAppointments { get; set; } // for child
    }
}
