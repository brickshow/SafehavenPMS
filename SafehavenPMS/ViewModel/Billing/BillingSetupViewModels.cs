using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel.Billing
{
    public class MonthlyFeeEditViewModel
    {
        [Display(Name="Treatment Fee"), Range(0, 999999)]
        public decimal TreatmentFee { get; set; }
        [Display(Name="Food Fee"), Range(0, 999999)]
        public decimal FoodFee { get; set; }
        [Display(Name="Accommodation & Amenities"), Range(0, 999999)]
        public decimal AccommodationAmenitiesFee { get; set; }
    }

    public class BankInfoEditViewModel
    {
        [Required, StringLength(120)]
        [Display(Name="Bank Name")]
        public string BankName { get; set; }
        [Required, StringLength(160)]
        [Display(Name="Account Name")]
        public string AccountName { get; set; }
        [Required, StringLength(80)]
        [Display(Name="Account Number")]
        public string AccountNumber { get; set; }
    }

    public class BillingSetupViewModel
    {
        public MonthlyFeeEditViewModel CurrentFees { get; set; }
        public decimal Total { get; set; }
        public BankInfoEditViewModel Bank { get; set; }
        public string EffectiveDateDisplay { get; set; }
    }
}