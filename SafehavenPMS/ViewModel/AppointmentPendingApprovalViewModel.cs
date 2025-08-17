namespace SafehavenPMS.ViewModel
{
    public class AppointmentPendingApprovalViewModel
    {
        public int AppointmentId { get; set; }
        public string VisitType { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; }
        public string DayName { get; set; }
        //public DateTime AppointmentDate { get; set; } //TODO
    }
}
