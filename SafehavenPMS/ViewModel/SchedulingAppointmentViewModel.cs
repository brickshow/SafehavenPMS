using System;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel
{
    public class ScheduleAppointmentViewModel
    {
        public int ScheduleId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int? ClinicalStaffID { get; set; }
        public string? ClinicalStaffName { get; set; }
        public string VisitType { get; set; }
        public DateTime SelectedDate { get; set; }
        public string TimeSlot { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
