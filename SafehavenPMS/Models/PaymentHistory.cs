using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    // Optional EF model — remove or adapt if you do not persist PaymentHistory separately.
    [Table("PaymentHistory")]
    public class PaymentHistory
    {
        [Key]
        public int PaymentHistoryId { get; set; }

        public int PaymentId { get; set; }
        public string? PaymentRefNumber { get; set; }
        public Payment? Payment { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        [MaxLength(50)]
        public string? InvoiceRefNumber { get; set; }

        [MaxLength(50)]
        public string? Period { get; set; }

        public int? Month { get; set; }
        public int? Year { get; set; }

        public DateTime? DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountToApply { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(200)]
        public string? RecordedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}