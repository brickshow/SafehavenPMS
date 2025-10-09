using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel.Canteen
{
    public class CreateCanteenPurchaseViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required, MaxLength(180)]
        public string ItemDescription { get; set; }

        [Range(1, 9999)]
        public int Quantity { get; set; }

        [Range(0.01, 999999)]
        public decimal UnitPrice { get; set; }
    }
}