namespace SafehavenPMS.ViewModel.Canteen
{
    public class CanteenPurchaseListItemViewModel
    {
        public int Id { get; set; }
        public string PurchaseRef => $"CAN-{Id:00000}";
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ItemDescription { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectedBy { get; set; }
    }
}