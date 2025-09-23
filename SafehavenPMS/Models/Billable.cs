using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafehavenPMS.Models
{
    public class Billable
    {
        [Key]
        public int BillableId { get; set; }

        public int? PatientId { get; set; } // optional for generic items
        public string Category { get; set; } = "Misc"; // "Medication" | "Canteen" | "Miscellaneous"
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0m;
        public decimal Total => UnitPrice * Quantity;
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
}