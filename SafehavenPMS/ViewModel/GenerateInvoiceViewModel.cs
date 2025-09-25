using System;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class GenerateInvoiceViewModel
    {
        [Required]
        [Range(1,12)]
        public int? Month { get; set; }

        [Required]
        public int? Year { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        // read-only display helpers (not posted)
        public string MonthName => Month.HasValue ? new DateTime(2000, Month.Value, 1).ToString("MMMM") : "";
    }
}