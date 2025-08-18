using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class MedicineViewModel
    {
        public int MedicineId { get; set; }
        [Required(ErrorMessage ="Generic name is required!")]
        public string GenericName { get; set; }
        [Required(ErrorMessage = "Brand name is required!")]
        public string BrandName { get; set; }

        [Required(ErrorMessage = "Medicine form is required!")]
        public string Form { get; set; }

        [Required(ErrorMessage = "Strength is required!")]
        public Decimal Strength { get; set; }

        [Required(ErrorMessage = "Unit is required!")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Price is required!")]
        public Decimal Price { get; set; }

        public string Status { get; set; } = "Active";
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
