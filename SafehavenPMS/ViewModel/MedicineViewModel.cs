using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class MedicineViewModel
    {
        public int MedicineId { get; set; }
        [Required(ErrorMessage ="Medicine name is required!")]
        public string MedicineName { get; set; }

        [Required(ErrorMessage = "Medicine form is required!")]
        public string Form { get; set; }

        [Required(ErrorMessage = "Dosage is required!")]
        public string Dosage { get; set; }

        [Required(ErrorMessage = "Price is required!")]
        public Decimal Price { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
