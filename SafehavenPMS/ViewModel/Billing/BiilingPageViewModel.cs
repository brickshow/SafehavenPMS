using System;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel.Billing
{
    public class BiilingPageViewModel
    {
        public List<BillableItemViewModel> Items { get; set; } = new();
        public int TotalCount => Items?.Count ?? 0;
    }
}