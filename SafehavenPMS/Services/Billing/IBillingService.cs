namespace SafehavenPMS.Services.Billing
{
    public interface IBillingService
    {
        Task AddBillableForCanteenPurchase(int patientId, int purchaseId, decimal amount, string description);
    }
}