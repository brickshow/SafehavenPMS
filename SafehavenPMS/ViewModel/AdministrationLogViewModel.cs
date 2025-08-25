namespace SafehavenPMS.ViewModel
{
    public class AdministrationLogViewModel
    {
        public int AdministrationLogId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int TotalMeds { get; set; }
        public string ScheduleTimes { get; set; }
        public string AdministrationStatus { get; set; }
    }
}
