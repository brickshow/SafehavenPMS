using System;
using System.Collections.Generic;
using SafehavenPMS.Models;

namespace SafehavenPMS.ViewModel.Billing
{
    public class BillableItemViewModel
    {
        public int? BillableId { get; set; }
        public int? MedicationId { get; set; }

        // add patient display name
        public string? PatientName { get; set; }

        public int? PatientId { get; set; }
        public string? CreatedBy { get; set; }
        public string? ReferenceType { get; set; } // optional tag: "MedicationOrder","Miscellaneous"
        public DateTime DateAdded { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

    public class BillablesPageViewModel
    {
        public int? PatientId { get; set; }
        public List<BillableItemViewModel> Items { get; set; } = new();
        public List<MiscellaneousItemViewModel> MiscellaneousItems { get; set; } = new();
        public MiscellaneousItemViewModel miscellaneousItemViewModel { get; set; } = new();
        public int TotalCount => Items?.Count ?? 0;
    }
}