namespace SafehavenPMS.Models
{
    public class Medicine
    {
        public int MedicineId { get; set; }
        public string GenericName { get; set; }
        public string BrandName { get; set; }
        public string Form { get; set; }
        public decimal Strength { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public DateTime DateAdded { get; set; }
        public string Status { get; set; } = "Active";
    }
}
