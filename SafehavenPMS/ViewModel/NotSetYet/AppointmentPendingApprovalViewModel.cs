using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class AppointmentPendingApprovalViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string VisitType { get; set; }
        public string Status { get; set; }
        public string DayName { get; set; }
        public string TimeSlot { get; set; }

        public DateTime AppointmentDate { get; set; }
        //public DateTime AppointmentDate { get; set; } //TODO
    }
}

