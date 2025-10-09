using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    public class CanteenPurchase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required, MaxLength(180)]
        public string ItemDescription { get; set; }

        [Range(1, 9999)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999)]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount => Quantity * UnitPrice;

        // Status: Pending Review | Approved | Rejected
        [Required, MaxLength(40)]
        public string Status { get; set; } = "Pending Review";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(120)]
        public string? CreatedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }
        [MaxLength(120)]
        public string? ApprovedBy { get; set; }

        public DateTime? RejectedAt { get; set; }
        [MaxLength(120)]
        public string? RejectedBy { get; set; }

        // Navigation
        public virtual Patient? Patient { get; set; }
    }
}