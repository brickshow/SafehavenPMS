using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel.Billing
{
    public class UploadPaymentViewModel
    {
        [Required]
        public int InvoiceId { get; set; }

        public int? PaymentId { get; set; }
        public int? PatientId { get; set; }
        public virtual Patient? Patient { get; set; }

        public string? PatientName { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Transaction No./Reference No.")]
        [StringLength(100)]
        public string? TransactionNumber { get; set; }

        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime? TransactionDate { get; set; }

        [Display(Name = "Amount Paid")]
        [Range(0, double.MaxValue, ErrorMessage = "Enter a valid amount")]
        public decimal? AmountPaid { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Display(Name = "Proof of Payment")]
        public IFormFile? ProofFile { get; set; }

        // Optional: store saved filename for UI/DB mapping
        public string? PhotoUrl { get; set; }

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string? CreatedBy { get; set; }
    }
}