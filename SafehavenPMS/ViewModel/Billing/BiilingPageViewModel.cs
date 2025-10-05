using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel.Billing
{
[Authorize]
    public class BiilingPageViewModel
    {
        public List<BillableItemViewModel> Items { get; set; } = new();
        public int TotalCount => Items?.Count ?? 0;
    }
}
