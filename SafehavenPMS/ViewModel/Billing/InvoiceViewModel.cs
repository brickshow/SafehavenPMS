using System;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel.Billing
{
    public class InvoiceListItemViewModel
    {
        public int InvoiceId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        // Period can be represented by Month/Year or a preformatted string
        public int? Month { get; set; }
        public int? Year { get; set; }

        // If you want to provide a custom period string, set this field via SetPeriod
        private string? _period;
        public string? Period => (Month.HasValue && Year.HasValue) ? new DateTime(Year.Value, Month.Value, 1).ToString("MMMM yyyy") : _period;
        public void SetPeriod(string period) => _period = period;

        public DateTime DueDate { get; set; }

        // Amount fields
        public decimal TotalAmount { get; set; }
        public decimal AmountDue { get; set; }

        // Status: "NotYetPaid", "Partial", "Paid", "Overdue", "Voided", etc.
        public string Status { get; set; } = "NotYetPaid";

        // Optional: number of lines and the detailed lines for the invoice (used by the UI)

        //Invoice lines
        public int InvoiceLineId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1m;
        public decimal UnitPrice { get; set; } = 0m;
        public decimal Amount { get; set; } = 0m;
        public DateTime DateAdded { get; set; }
        public int? ReferenceBillableId { get; set; }
    }
}