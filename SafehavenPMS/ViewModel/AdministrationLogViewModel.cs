namespace SafehavenPMS.ViewModel
{
    public class AdministrationLogViewModel
    {
        public int AdministrationLogId { get; set; }
        public int MedicationOrderId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int TotalMeds { get; set; }
        public string ScheduleTimes { get; set; }
        public string AdministrationStatus { get; set; }

        // List of medications for this patient
        public List<MedicationOrderViewModel> Medications { get; set; }
    }
}
