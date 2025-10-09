using System.Collections.Generic;

namespace SafehavenPMS.ViewModel.Canteen
{
    public class CanteenIndexViewModel
    {
        public IEnumerable<CanteenPurchaseListItemViewModel> Items { get; set; } = new List<CanteenPurchaseListItemViewModel>();

        public string SearchQuery { get; set; }
        public string StatusFilter { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; } // asc | desc
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => PageSize == 0 ? 1 : (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}