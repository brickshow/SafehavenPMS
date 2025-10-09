namespace SafehavenPMS.ViewModel.Dashboard
{
    public class DashboardStatsViewModel
    {
        public int TotalPatients { get; set; }
        public int NewPatientsThisMonth { get; set; }
        public int Doctors { get; set; }
        public int Nurses { get; set; }
        public int Coaches { get; set; }
        public int Appointments { get; set; }
        public int Invoices { get; set; }
        public int Users { get; set; }
    }
}