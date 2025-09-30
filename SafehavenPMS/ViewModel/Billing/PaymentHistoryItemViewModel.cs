using System;

namespace SafehavenPMS.ViewModel.Billing
{
    public class PaymentHistoryItemViewModel
    {
        public int PaymentHistoryId { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public int PaymentId { get; set; }

        public int InvoiceId { get; set; }
        public string? PaymentRefNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public string InvoiceRefNumber { get; set; } = string.Empty;
        // Displayable period (e.g. "Sep 2023") — view falls back to Month/Year if empty
        public string? Period { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountToApply { get; set; }

        public string? Remarks { get; set; }

        // who recorded the payment (user name)
        public string? RecordedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}