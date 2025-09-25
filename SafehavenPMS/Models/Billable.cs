using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    public class Billable
    {
        [Key]
        public int BillableId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(64)]
        public string Category { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        public decimal Quantity { get; set; } = 1m;
        public decimal UnitPrice { get; set; } = 0m;

        // stored computed amount = Quantity * UnitPrice (set in code before save)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public int? ReferenceId { get; set; }   // optional FK to source record (MedicationOrderId, MiscId, etc.)
        public string? ReferenceType { get; set; } // optional tag: "MedicationOrder","Miscellaneous"
    
        public Patient? Patient { get; set; }
    }
}