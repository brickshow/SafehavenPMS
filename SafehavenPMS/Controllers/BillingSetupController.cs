using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models.Billing;
using SafehavenPMS.Models.Audit;
using SafehavenPMS.ViewModel.Billing;

namespace SafehavenPMS.Controllers
{
    [Authorize(Roles="Admin")]
    public class BillingSetupController : Controller
    {
        private readonly SafehavenPMSContext _ctx;
        public BillingSetupController(SafehavenPMSContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var latestFees = await _ctx.BillingMonthlyFees
                .OrderByDescending(f => f.EffectiveDate)
                .FirstOrDefaultAsync();

            var bank = await _ctx.BankInformations
                .OrderByDescending(b => b.UpdatedAt)
                .FirstOrDefaultAsync();

            var vm = new BillingSetupViewModel
            {
                CurrentFees = latestFees == null
                    ? new MonthlyFeeEditViewModel()
                    : new MonthlyFeeEditViewModel
                    {
                        TreatmentFee = latestFees.TreatmentFee,
                        FoodFee = latestFees.FoodFee,
                        AccommodationAmenitiesFee = latestFees.AccommodationAmenitiesFee
                    },
                Total = latestFees?.TreatmentFee + latestFees?.FoodFee + latestFees?.AccommodationAmenitiesFee ?? 0,
                Bank = bank == null
                    ? new BankInfoEditViewModel()
                    : new BankInfoEditViewModel
                    {
                        BankName = bank.BankName,
                        AccountName = bank.AccountName,
                        AccountNumber = bank.AccountNumber
                    },
                EffectiveDateDisplay = latestFees?.EffectiveDate.ToLocalTime().ToString("MMM dd, yyyy") ?? "-"
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditMonthlyFee()
        {
            var latest = await _ctx.BillingMonthlyFees
                .OrderByDescending(f => f.EffectiveDate)
                .FirstOrDefaultAsync();
            return View(latest == null
                ? new MonthlyFeeEditViewModel()
                : new MonthlyFeeEditViewModel
                {
                    TreatmentFee = latest.TreatmentFee,
                    FoodFee = latest.FoodFee,
                    AccommodationAmenitiesFee = latest.AccommodationAmenitiesFee
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMonthlyFee(MonthlyFeeEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = new BillingMonthlyFee
            {
                TreatmentFee = model.TreatmentFee,
                FoodFee = model.FoodFee,
                AccommodationAmenitiesFee = model.AccommodationAmenitiesFee,
                EffectiveDate = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            _ctx.BillingMonthlyFees.Add(entity);
            _ctx.AuditLogs.Add(new AuditLog
            {
                Actor = User.Identity?.Name ?? "System",
                Action = "Update Monthly Fees",
                Module = "BillingSetup",
                Details = $"Treatment={model.TreatmentFee};Food={model.FoodFee};Accommodation={model.AccommodationAmenitiesFee};Total={entity.Total}"
            });
            await _ctx.SaveChangesAsync();
            TempData["SuccessMessage"] = "Monthly fees updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditBank()
        {
            var bank = await _ctx.BankInformations
                .OrderByDescending(b => b.UpdatedAt)
                .FirstOrDefaultAsync();
            return View(bank == null
                ? new BankInfoEditViewModel()
                : new BankInfoEditViewModel
                {
                    BankName = bank.BankName,
                    AccountName = bank.AccountName,
                    AccountNumber = bank.AccountNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBank(BankInfoEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Store as single row (replace or create)
            var existing = await _ctx.BankInformations
                .OrderByDescending(b => b.UpdatedAt)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                existing = new BankInformation
                {
                    BankName = model.BankName,
                    AccountName = model.AccountName,
                    AccountNumber = model.AccountNumber,
                    UpdatedBy = User.Identity?.Name ?? "System"
                };
                _ctx.BankInformations.Add(existing);
            }
            else
            {
                existing.BankName = model.BankName;
                existing.AccountName = model.AccountName;
                existing.AccountNumber = model.AccountNumber;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = User.Identity?.Name ?? "System";
                _ctx.BankInformations.Update(existing);
            }

            _ctx.AuditLogs.Add(new AuditLog
            {
                Actor = User.Identity?.Name ?? "System",
                Action = "Update Bank Info",
                Module = "BillingSetup",
                Details = $"Bank={model.BankName};AccountName={model.AccountName};AccountNo={model.AccountNumber}"
            });

            await _ctx.SaveChangesAsync();
            TempData["SuccessMessage"] = "Bank details updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}