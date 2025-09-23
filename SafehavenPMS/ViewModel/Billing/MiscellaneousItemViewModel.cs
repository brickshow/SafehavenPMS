namespace SafehavenPMS.ViewModel.Billing
{
    public class MiscellaneousItemViewModel
    {
        public int PatientId { get; set; }
        public List<string> ItemDescriptions { get; set; }
        public List<decimal> Amounts { get; set; }
        public decimal Total { get; set; }
    }
}
