using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.ViewModel.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SafehavenPMS.Controllers
{
    public partial class BillingController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public BillingController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var billables = await _context.Billables
                                .Include(p => p.Patient)
                                .ToListAsync();

            // var medItems = medOrders.Select(b => new BillableItemViewModel
            // {
            //     PatientId = b.PatientId,
            //     PatientName = b.Patient != null ? $"{b.Patient.Firstname} {b.Patient.Lastname}" : null,
            //     MedicationId = b.MedicineId,
            //     Category = "Medication",
            //     Description = $"{b.Medicine?.GenericName}",
            //     Quantity = b.UnitPerDose,
            //     UnitPrice = b.Medicine?.Price ?? 0m,
            //     DateAdded = b.CreatedAt,
            //     CreatedBy = b.CreatedBy
            // }).ToList();

            // // map miscellaneous items to the same view-model shape
            // var misc = await _context.MiscellaneousItems
            //                 .Include(m => m.Patient)
            //                 .ToListAsync();

            // var miscItems = billables.Select(m => new BillableItemViewModel
            // {
            //     PatientId = m.PatientId,
            //     PatientName = m.Patient != null ? $"{m.Patient.Firstname} {m.Patient.Lastname}" : null,
            //     MedicationId = null,
            //     Category = "Miscellaneous",
            //     Description = m.Description,
            //     Quantity = 1,
            //     UnitPrice = m.Amount,
            //     CreatedBy = m.CreatedBy
            // }).ToList();

            var viewModel = new BillablesPageViewModel
            
            {
                Items = billables.Select(b => new BillableItemViewModel
                {
                    PatientId = b.PatientId,
                    PatientName = b.Patient != null ? $"{b.Patient.Firstname} {b.Patient.Lastname}" : null,
                    ReferenceType = b.ReferenceType,
                    Category = b.Category,
                    Description = b.Description,
                    Quantity = b.Quantity,
                    UnitPrice = b.Amount,
                    CreatedBy = b.CreatedBy
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: /Billing/AddMiscellaneousItem
        [HttpGet]
        public IActionResult AddMiscellaneousItem()
        {
            var vm = new MiscellaneousItemViewModel();

            // ensure lists are initialized so view's indexed asp-for (ItemDescriptions[0], Amounts[0]) won't throw
            vm.ItemDescriptions = new System.Collections.Generic.List<string> { string.Empty };
            vm.Amounts = new System.Collections.Generic.List<decimal> { 0m };

            return View(vm); // returns Views/Billing/AddMiscellaneousItem.cshtml
        }

        // POST: /Billing/AddMiscellaneousItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMiscellaneousItem(SafehavenPMS.ViewModel.Billing.MiscellaneousItemViewModel vm)
        {
            if (vm == null)
                return BadRequest();

            // normalize lists so view rendering after validation errors is safe
            vm.ItemDescriptions = vm.ItemDescriptions ?? new System.Collections.Generic.List<string>();
            vm.Amounts = vm.Amounts ?? new System.Collections.Generic.List<decimal>();

            if (vm.ItemDescriptions.Count == 0)
            {
                vm.ItemDescriptions.Add(string.Empty);
            }
            if (vm.Amounts.Count == 0)
            {
                vm.Amounts.Add(0m);
            }

            if (vm.PatientId <= 0) ModelState.AddModelError(nameof(vm.PatientId), "Please select a patient.");

            if (vm.ItemDescriptions == null || vm.Amounts == null || vm.ItemDescriptions.Count == 0)
                ModelState.AddModelError(string.Empty, "Please add at least one item.");

            if (vm.ItemDescriptions != null && vm.Amounts != null && vm.ItemDescriptions.Count != vm.Amounts.Count)
                ModelState.AddModelError(string.Empty, "Item descriptions and amounts count mismatch.");

            if (vm.ItemDescriptions != null && vm.Amounts != null)
            {
                for (int i = 0; i < vm.ItemDescriptions.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(vm.ItemDescriptions[i])) ModelState.AddModelError($"ItemDescriptions[{i}]", "Description required.");
                    if (vm.Amounts[i] <= 0) ModelState.AddModelError($"Amounts[{i}]", "Amount must be > 0.");
                }
            }

            if (!ModelState.IsValid) return View(vm);

            var patient = await _context.Patients.FindAsync(vm.PatientId);
            if (patient == null) { ModelState.AddModelError(nameof(vm.PatientId), "Patient not found."); return View(vm); }

            var now = DateTime.Now;
            var createdBy = User?.Identity?.Name ?? "system";

            var miscEntities = new List<SafehavenPMS.Models.MiscellaneousItem>();
            for (int i = 0; i < vm.ItemDescriptions.Count; i++)
            {
                var desc = (vm.ItemDescriptions[i] ?? string.Empty).Trim();
                var amt = vm.Amounts[i];
                if (string.IsNullOrEmpty(desc) || amt <= 0) continue;

                miscEntities.Add(new SafehavenPMS.Models.MiscellaneousItem
                {
                    PatientId = vm.PatientId,
                    Description = desc,
                    Amount = amt,
                    CreatedAt = now,
                    CreatedBy = createdBy
                });
            }

            if (miscEntities.Count == 0) { ModelState.AddModelError(string.Empty, "No valid items to save."); return View(vm); }

            // Save miscellaneous items first so we have their DB ids
            _context.MiscellaneousItems.AddRange(miscEntities);
            await _context.SaveChangesAsync();

            // Create corresponding Billable entries for each saved miscellaneous item.
            // ReferenceId will point to the MiscellaneousItem.Id and ReferenceType will hold the formatted code "BILL-00001"
            var billableEntities = new List<SafehavenPMS.Models.Billable>();
            foreach (var m in miscEntities)
            {
                var bill = new SafehavenPMS.Models.Billable
                {
                    PatientId = m.PatientId,
                    Category = "Miscellaneous",
                    Description = m.Description,
                    Quantity = 1m,
                    UnitPrice = m.Amount,
                    Amount = m.Amount,
                    DateAdded = m.CreatedAt,
                    CreatedBy = m.CreatedBy,
                    ReferenceId = m.Id,
                    ReferenceType = $"BILL-{m.Id:D5}"   // e.g. BILL-00001
                };
                billableEntities.Add(bill);
            }

            if (billableEntities.Count > 0)
            {
                _context.Billables.AddRange(billableEntities);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"{miscEntities.Count} miscellaneous item(s) added and saved as billables.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Billing/SearchPatients?q=term
        [HttpGet]
        public async Task<IActionResult> SearchPatients(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(Array.Empty<object>());

            q = q.Trim().ToLowerInvariant();

            var results = await _context.Patients
                .Where(p =>
                    (((p.Firstname ?? "") + " " + (p.Lastname ?? "")).ToLower().Contains(q)) ||
                    ((p.Firstname ?? "").ToLower().Contains(q)) ||
                    ((p.Lastname ?? "").ToLower().Contains(q))
                )
                .OrderBy(p => p.Lastname)
                .ThenBy(p => p.Firstname)
                .Select(p => new
                {
                    id = p.PatientId,
                    text = (((p.Firstname ?? "") + " " + (p.Lastname ?? "")).Replace("  ", " ")).Trim(),
                })
                .Take(20)
                .ToListAsync();

            return Json(results);
        }

        [HttpGet]
        public IActionResult GenerateInvoice()
        {
            var vm = new GenerateInvoiceViewModel
            {
                Year = DateTime.UtcNow.Year,
                DueDate = DateTime.UtcNow.Date
            };

            // read TempData messages (view will display them)
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateInvoiceConfirm(GenerateInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("GenerateInvoice", model);
            }

            // show confirmation view
            return View("GenerateInvoiceConfirm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateInvoice(GenerateInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
                return View("GenerateInvoice", model);

            // server-side check whether invoices for that month/year already exist
            // bool exists = await _context.Set<Invoice>()
            //     .AnyAsync(i => i.Month == model.Month && i.Year == model.Year);

            string displayPeriod = $"{model.MonthName} {model.Year}";

            // if (exists)
            // {
            //     TempData["Warning"] = $"Invoices for {displayPeriod} already exist.";
            //     return RedirectToAction(nameof(GenerateInvoice));
            // }

            // TODO: replace this placeholder with your real invoice creation logic
            // Example: create invoices for patients who should be billed
            int createdCount = 0;
            var billablePatients = await _context.Set<Patient>().Where(p => /* your criteria */ true).ToListAsync();

            // foreach (var p in billablePatients)
            // {
            //     var invoice = new Invoice
            //     {
            //         PatientId = p.Id,
            //         Month = model.Month.Value,
            //         Year = model.Year.Value,
            //         DueDate = model.DueDate.Value,
            //         CreatedAt = DateTime.UtcNow,
            //         Amount = 0m // compute real amount
            //     };
            //     _context.Add(invoice);
            //     createdCount++;
            // }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Monthly Invoices Generated Successfully for {displayPeriod}";
            TempData["GeneratedCount"] = createdCount.ToString();
            return RedirectToAction(nameof(GenerateInvoice));
        }
    }
}