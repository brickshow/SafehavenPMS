using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;

namespace SafehavenPMS.Services.Billing
{
    public class BillingService : IBillingService
    {
        private readonly SafehavenPMSContext _context;

        public BillingService(SafehavenPMSContext context)
        {
            _context = context;
        }

        // TODO: Persist a billable record (create Billable entity if not existing)
        public async Task AddBillableForCanteenPurchase(int patientId, int purchaseId, decimal amount, string description)
        {
            bool exists = await _context.Billables.AnyAsync(b =>
                b.ReferenceType == "CanteenPurchase" && b.ReferenceId == purchaseId);

            if (exists) return;

            var billable = new Billable
            {
                PatientId = patientId,
                Category = "Canteen",
                Description = description,
                Quantity = 1m,
                UnitPrice = amount,
                Amount = amount,
                ReferenceId = purchaseId,
                ReferenceType = "CanteenPurchase",
                CreatedBy = "BillingService"
            };

            _context.Billables.Add(billable);
            await _context.SaveChangesAsync();
        }
    }
}