namespace SafehavenPMS.ViewModel
{
    public class MedicationOrderViewModel
    {
        public int MedicationOrderId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public decimal Dose { get; set; }
        public string Unit { get; set; }
        public string Form { get; set; }
        public string Instruction { get; set; }
        public string Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
