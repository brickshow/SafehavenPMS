namespace SafehavenPMS.Models
{
    public class Medicine
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public string Form { get; set; }
        public string Dosage { get; set; }
        public Decimal Price { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
