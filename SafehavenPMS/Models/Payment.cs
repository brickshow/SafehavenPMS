using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public string? PaymentRefId { get; set; }

        public int InvoiceId { get; set; }
        public virtual Invoice? Invoice { get; set; }

        // Added patient FK for convenience (nullable to avoid migration issues if you prefer non-nullable make int)
        public int? PatientId { get; set; }
        public virtual Patient? Patient { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(200)]
        public string? TransactionNumber { get; set; }

        public DateTime? TransactionDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public string status { get; set; } = "Pending Review";

        [MaxLength(300)] 
        public string? ProofFileName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(200)]
        public string? CreatedBy { get; set; }
    }
}
