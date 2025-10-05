using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
        public string? InvoiceRefId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(64)]
        public string InvoiceNumber { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }

        public string? Status { get; set; }

        // computed
        public decimal TotalAmount { get; set; }

        public Patient? Patient { get; set; }

        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }

    public class InvoiceLine
    {
        [Key]
        public int InvoiceLineId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(64)]
        public string Category { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        public decimal Quantity { get; set; } = 1m;
        public decimal UnitPrice { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Optional reference to original Billable (if any)
        public int? ReferenceBillableId { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        public Invoice? Invoice { get; set; }
    }
}
