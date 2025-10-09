using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models.Billing
{
    public class BillingMonthlyFee
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName="decimal(18,2)")]
        [Range(0, 999999)]
        public decimal TreatmentFee { get; set; }

        [Column(TypeName="decimal(18,2)")]
        [Range(0, 999999)]
        public decimal FoodFee { get; set; }

        [Column(TypeName="decimal(18,2)")]
        [Range(0, 999999)]
        public decimal AccommodationAmenitiesFee { get; set; }

        [NotMapped]
        public decimal Total => TreatmentFee + FoodFee + AccommodationAmenitiesFee;

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(100)]
        public string CreatedBy { get; set; }
    }
}