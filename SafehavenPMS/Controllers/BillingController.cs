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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SafehavenPMS.Services;

namespace SafehavenPMS.Controllers
{
[Authorize]
    public partial class BillingController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly CloudinaryServices _cloudinary;
        public BillingController(SafehavenPMSContext context, CloudinaryServices cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<IActionResult> Index(
    string billableSearch = null,
    string invoiceSearch = null,
    string invoiceStatus = null,
    string paymentSearch = null,
    string paymentStatus = null,
    string historyFrom = null,
    string historyTo = null)
{
    var userIsFamily = User.IsInRole("Family");
    var today = DateTime.UtcNow.Date;

    // Placeholder: resolve accessible patient ids for Family user
    // TODO: replace with actual linkage (e.g. FamilyPatient table)
    IQueryable<Patient> patientQuery = _context.Patients.AsQueryable();
    if (userIsFamily)
    {
        // restrict to none (until linkage implemented)
        patientQuery = patientQuery.Where(p => false);
    }

    var patientIds = await patientQuery.Select(p => p.PatientId).ToListAsync();

    // Billables
    var billablesQuery = _context.Billables
        .Include(b => b.Patient)
        .AsQueryable();

    if (userIsFamily)
        billablesQuery = billablesQuery.Where(b => patientIds.Contains(b.PatientId));

    if (!string.IsNullOrWhiteSpace(billableSearch))
    {
        var s = billableSearch.Trim().ToLower();
        billablesQuery = billablesQuery.Where(b =>
            b.Description.ToLower().Contains(s) ||
            b.PatientId.ToString().Contains(s) ||
            ((b.Patient.Firstname ?? "") + " " + (b.Patient.Lastname ?? "")).ToLower().Contains(s));
    }

    var billables = await billablesQuery
        .OrderByDescending(b => b.DateAdded)
        .ToListAsync();

    // Invoices
    var invoicesQuery = _context.Invoices
        .Include(i => i.Patient)
        .Include(i => i.Lines)
        .AsQueryable();

    if (userIsFamily)
        invoicesQuery = invoicesQuery.Where(i => patientIds.Contains(i.PatientId));

    if (!string.IsNullOrWhiteSpace(invoiceSearch))
    {
        var s = invoiceSearch.Trim().ToLower();
        invoicesQuery = invoicesQuery.Where(i =>
            i.InvoiceNumber.ToLower().Contains(s) ||
            i.PatientId.ToString().Contains(s) ||
            ((i.Patient.Firstname ?? "") + " " + (i.Patient.Lastname ?? "")).ToLower().Contains(s));
    }

    var invoicesRaw = await invoicesQuery
        .OrderByDescending(i => i.CreatedAt)
        .ToListAsync();

    // Payments
    var paymentsQuery = _context.Payments
        .Include(p => p.Patient)
        .Include(p => p.Invoice)
        .AsQueryable();

    if (userIsFamily)
        paymentsQuery = paymentsQuery.Where(p => patientIds.Contains(p.PatientId ?? 0));

    if (!string.IsNullOrWhiteSpace(paymentSearch))
    {
        var s = paymentSearch.Trim().ToLower();
        paymentsQuery = paymentsQuery.Where(p =>
            (p.TransactionNumber ?? "").ToLower().Contains(s) ||
            p.PaymentRefId.ToLower().Contains(s) ||
            (p.Invoice.InvoiceNumber ?? "").ToLower().Contains(s) ||
            p.InvoiceId.ToString().Contains(s) ||
            ((p.Patient.Firstname ?? "") + " " + (p.Patient.Lastname ?? "")).ToLower().Contains(s));
    }

    if (!string.IsNullOrWhiteSpace(paymentStatus))
        paymentsQuery = paymentsQuery.Where(p => p.status == paymentStatus);

    var payments = await paymentsQuery
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();

    // Payment History (date range filters)
    DateTime? fromDate = null;
    DateTime? toDate = null;
    if (DateTime.TryParse(historyFrom, out var fd)) fromDate = fd.Date;
    if (DateTime.TryParse(historyTo, out var td)) toDate = td.Date.AddDays(1).AddTicks(-1);

    var historyQuery = _context.PaymentHistories
        .Include(ph => ph.Invoice)
        .Include(ph => ph.Payment)
        .ThenInclude(p => p.Patient)
        .AsQueryable();

    if (userIsFamily)
        historyQuery = historyQuery.Where(h => patientIds.Contains(h.Invoice.PatientId));

    if (fromDate.HasValue)
        historyQuery = historyQuery.Where(h => h.CreatedAt >= fromDate.Value);
    if (toDate.HasValue)
        historyQuery = historyQuery.Where(h => h.CreatedAt <= toDate.Value);

    var paymentHistories = await historyQuery
        .OrderByDescending(h => h.CreatedAt)
        .ToListAsync();

    // Preload verified payments per invoice for invoice status computation
    var verifiedPaymentsPerInvoice = await _context.Payments
        .Where(p => p.status == "Verified")
        .GroupBy(p => p.InvoiceId)
        .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(x => x.AmountPaid) })
        .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

    var invoiceVMs = invoicesRaw.Select(inv =>
    {
        verifiedPaymentsPerInvoice.TryGetValue(inv.InvoiceId, out var paid);
        paid = paid < 0 ? 0 : paid;

        var amountDue = Math.Max(0m, inv.TotalAmount - paid);
        string derivedStatus;
        if (amountDue == 0 && inv.TotalAmount > 0) derivedStatus = "Paid";
        else if (paid > 0 && amountDue > 0) derivedStatus = "Partial";
        else if (paid == 0 && inv.DueDate.Date < today) derivedStatus = "Overdue";
        else derivedStatus = "NotYetPaid";

        // honor stored status if it is "Voided"
        if (string.Equals(inv.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            derivedStatus = "Voided";

        return new InvoiceListItemViewModel
        {
            InvoiceId = inv.InvoiceId,
            PatientId = inv.PatientId,
            PatientName = inv.Patient != null ? $"{inv.Patient.Firstname} {inv.Patient.Lastname}" : string.Empty,
            InvoiceNumber = inv.InvoiceNumber,
            Month = inv.Month,
            Year = inv.Year,
            DueDate = inv.DueDate,
            TotalAmount = inv.TotalAmount,
            AmountDue = amountDue,
            Status = derivedStatus
        };
    }).ToList();

    if (!string.IsNullOrWhiteSpace(invoiceStatus))
        invoiceVMs = invoiceVMs.Where(i => i.Status.Equals(invoiceStatus, StringComparison.OrdinalIgnoreCase)).ToList();

    var viewModel = new BillablesPageViewModel
    {
        Items = billables.Select(b => new BillableItemViewModel
        {
            BillableId = b.BillableId,
            PatientId = b.PatientId,
            PatientName = b.Patient != null ? $"{b.Patient.Firstname} {b.Patient.Lastname}" : string.Empty,
            ReferenceType = b.ReferenceType,
            Category = b.Category,
            Description = b.Description,
            Quantity = b.Quantity,
            UnitPrice = b.UnitPrice,
            CreatedBy = b.CreatedBy,
            DateAdded = b.DateAdded
        }).ToList(),
        Invoices = invoiceVMs,
        UploadPayments = payments.Select(p => new UploadPaymentViewModel
        {
            PaymentId = p.PaymentId,
            InvoiceId = p.InvoiceId,
            PatientId = p.PatientId,
            PatientName = p.Patient != null ? $"{p.Patient.Firstname} {p.Patient.Lastname}" : string.Empty,
            AmountPaid = p.AmountPaid,
            TransactionNumber = p.TransactionNumber,
            PaymentMethod = p.PaymentMethod,
            PhotoUrl = p.ProofFileName,
            Status = p.status,
            Remarks = p.Remarks,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy
        }).ToList(),
        PaymentHistories = paymentHistories.Select(ph => new PaymentHistoryItemViewModel
        {
            PaymentHistoryId = ph.PaymentHistoryId,
            PaymentId = ph.PaymentId,
            PaymentRefNumber = ph.Payment?.PaymentRefId,
            InvoiceId = ph.InvoiceId,
            InvoiceRefNumber = ph.Invoice?.InvoiceRefId,
            Period = ph.Period,
            Month = ph.Month,
            Year = ph.Year,
            DueDate = ph.DueDate,
            TotalAmount = ph.TotalAmount,
            AmountDue = ph.AmountDue,
            AmountToApply = ph.AmountToApply,
            Remarks = ph.Remarks,
            RecordedBy = ph.RecordedBy,
            CreatedAt = ph.CreatedAt
        }).ToList()
    };

    ViewBag.Filters = new {
        billableSearch,
        invoiceSearch,
        invoiceStatus,
        paymentSearch,
        paymentStatus,
        historyFrom,
        historyTo
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
        public async Task<IActionResult> GenerateInvoice()
        {
            var vm = new GenerateInvoiceViewModel
            {
                Year = DateTime.UtcNow.Year,
                DueDate = DateTime.UtcNow.Date
            };

            var latestFees = await _context.BillingMonthlyFees
                                .OrderByDescending(f => f.EffectiveDate)
                                .FirstOrDefaultAsync();

            ViewBag.TreatmentFee = latestFees?.TreatmentFee ?? 0m;
            ViewBag.FoodFee = latestFees?.FoodFee ?? 0m;
            ViewBag.AccommodationAmenitiesFee = latestFees?.AccommodationAmenitiesFee ?? 0m;
            ViewBag.TotalMonthlyFee = (latestFees?.TreatmentFee ?? 0m)
                                    + (latestFees?.FoodFee ?? 0m)
                                    + (latestFees?.AccommodationAmenitiesFee ?? 0m);
            ViewBag.HasMonthlyFeeConfig = latestFees != null;
            ViewBag.EffectiveDate = latestFees?.EffectiveDate.ToLocalTime().ToString("MMM dd, yyyy");

            return View(vm);
        }

        // POST: generate invoices using the configured monthly fee (no manual entry)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateInvoiceConfirm(GenerateInvoiceViewModel model)
        {
            if (model == null) return BadRequest();

            if (!model.Month.HasValue || !model.Year.HasValue)
            {
                TempData["Warning"] = "Please select month and year.";
                return RedirectToAction(nameof(GenerateInvoice));
            }

            var latestFees = await _context.BillingMonthlyFees
                                .OrderByDescending(f => f.EffectiveDate)
                                .FirstOrDefaultAsync();

            if (latestFees == null)
            {
                TempData["Warning"] = "Monthly fee not configured. Set it up first.";
                return RedirectToAction("Index", "BillingSetup");
            }

            var today = DateTime.UtcNow.Date;
            int month = model.Month.Value;
            int year = model.Year.Value;

            // Rule: Only completed past months allowed
            if (year > today.Year || (year == today.Year && month >= today.Month))
            {
                TempData["Warning"] = "You may only generate invoices for completed past months.";
                return RedirectToAction(nameof(GenerateInvoice));
            }

            DateTime periodStart = new DateTime(year, month, 1);
            DateTime periodEnd = periodStart.AddMonths(1).AddTicks(-1);

            // Due date must be ≥ 15 days from generation date
            DateTime dueDate = model.DueDate ?? today;
            if (dueDate < today.AddDays(15))
            {
                TempData["Warning"] = "Due Date must be at least 15 days after today.";
                return RedirectToAction(nameof(GenerateInvoice));
            }

            decimal standardFee = latestFees.TreatmentFee
                                + latestFees.FoodFee
                                + latestFees.AccommodationAmenitiesFee;

            bool anyBillables = await _context.Billables
                .AnyAsync(b => b.DateAdded >= periodStart && b.DateAdded <= periodEnd);

            if (!anyBillables && standardFee <= 0m)
            {
                TempData["Warning"] = "No billables found for the selected period.";
                return RedirectToAction(nameof(GenerateInvoice));
            }

            string createdBy = User?.Identity?.Name ?? "system";

            var invoices = await BillingHelper.GenerateMonthlyInvoicesAsync(
                _context,
                month,
                year,
                standardFee,
                dueDate,
                createdBy,
                persist: true);

            TempData["Success"] = $"Monthly invoices generated for {periodStart:MMMM yyyy}.";
            TempData["GeneratedCount"] = invoices.Count;
            return RedirectToAction("Index");
        }

        // GET: /Billing/Invoice/5
        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            if (id <= 0) return RedirectToAction(nameof(Index));

            var inv = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (inv == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }

            var firstLine = inv.Lines.OrderBy(l => l.DateAdded).FirstOrDefault();

            var vm = new InvoiceListItemViewModel
            {
                InvoiceId = inv.InvoiceId,
                PatientId = inv.PatientId,
                PatientName = inv.Patient != null ? $"{inv.Patient.Firstname} {inv.Patient.Lastname}" : string.Empty,
                InvoiceNumber = inv.InvoiceNumber ?? $"INV-{inv.InvoiceId:D6}",
                Month = inv.Month,
                Year = inv.Year,
                DueDate = inv.DueDate,
                TotalAmount = inv.TotalAmount,
                AmountDue = inv.TotalAmount, // replace with remaining-balance calculation if you track payments
                Status = inv.Status ?? "NotYetPaid",

                // keep single-line compatibility fields populated
                InvoiceLineId = firstLine?.InvoiceLineId ?? 0,
                Category = firstLine?.Category ?? string.Empty,
                Description = firstLine?.Description ?? string.Empty,
                Quantity = firstLine?.Quantity ?? 1m,
                UnitPrice = firstLine?.UnitPrice ?? 0m,
                Amount = firstLine?.Amount ?? 0m,
                DateAdded = firstLine?.DateAdded ?? DateTime.UtcNow,
                ReferenceBillableId = firstLine?.ReferenceBillableId
            };

            // prepare invoice lines for view (anonymous projection)
            var lines = inv.Lines
                  .OrderBy(l => l.Category)
                  .ThenBy(l => l.DateAdded)
                  .Select(l => new
                  {
                      InvoiceLineId = l.InvoiceLineId,
                      Category = l.Category,
                      Description = l.Description,
                      Quantity = l.Quantity,
                      UnitPrice = l.UnitPrice,
                      Amount = l.Amount,
                      DateAdded = l.DateAdded,
                      ReferenceBillableId = l.ReferenceBillableId
                  })
                  .ToList();

            ViewBag.Lines = lines;

            // prepare billables for the same patient/period (if month/year present on invoice)
            List<object> billables;
            // inv.Month and inv.Year are integers (not nullable) so check their values directly.
            if (inv.Month > 0 && inv.Year > 0)
            {
                var periodStart = new DateTime(inv.Year, inv.Month, 1);
                var periodEnd = periodStart.AddMonths(1).AddTicks(-1);

                billables = await _context.Billables
                    .Where(b => b.PatientId == inv.PatientId && b.DateAdded >= periodStart && b.DateAdded <= periodEnd)
                    .OrderBy(b => b.Category)
                    .ThenBy(b => b.DateAdded)
                    .Select(b => new
                    {
                        BillableId = b.BillableId,
                        DateAdded = b.DateAdded,
                        Description = b.Description,
                        Category = b.Category,
                        Quantity = b.Quantity,
                        UnitPrice = b.UnitPrice,
                        Amount = b.Amount
                    })
                    .ToListAsync<object>();
            }
            else
            {
                // fallback: include all billables for patient
                billables = await _context.Billables
                    .Where(b => b.PatientId == inv.PatientId)
                    .OrderBy(b => b.Category)
                    .ThenBy(b => b.DateAdded)
                    .Select(b => new
                    {
                        BillableId = b.BillableId,
                        DateAdded = b.DateAdded,
                        Description = b.Description,
                        Category = b.Category,
                        Quantity = b.Quantity,
                        UnitPrice = b.UnitPrice,
                        Amount = b.Amount
                    })
                    .ToListAsync<object>();
            }

            ViewBag.Billables = billables;

            return View("Invoice", vm);
        }

        // Optional: Download PDF stub (implement PDF generation as needed)
        [HttpGet]
        public async Task<IActionResult> DownloadInvoicePdf(int id)
        {
            var inv = await _context.Invoices.Include(i => i.Lines).Include(i => i.Patient).FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (inv == null) return NotFound();

            // TODO: generate PDF bytes from Invoice view or template
            byte[] pdfBytes = System.Text.Encoding.UTF8.GetBytes($"Invoice PDF for {inv.InvoiceNumber ?? inv.InvoiceId.ToString()} (stub)");
            return File(pdfBytes, "application/pdf", $"{inv.InvoiceNumber ?? $"invoice-{inv.InvoiceId}"}.pdf");
        }

        // GET: Billing/UploadPayment/5
        [HttpGet]
        public IActionResult UploadPayment(int id)
        {
            if (id <= 0) return RedirectToAction(nameof(Index));

            var vm = new UploadPaymentViewModel
            {
                InvoiceId = id,
                TransactionDate = DateTime.Today,
                PaymentMethod = "BPI" // default selection
            };

            return View(vm);
        }

        // POST: Billing/UploadPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPayment(UploadPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? proofUrl = null;

            if (model.ProofFile != null && model.ProofFile.Length > 0)
            {
                using (var stream = model.ProofFile.OpenReadStream())
                {
                    // use the dedicated receipt upload method
                    proofUrl = await _cloudinary.UploadReceiptAsync(stream, model.ProofFile.FileName);
                }
            }

            // persist payment record
            var payment = new Payment
            {
                InvoiceId = model.InvoiceId,
                // set PatientId from invoice to keep in sync
                PatientId = (await _context.Invoices.Where(i => i.InvoiceId == model.InvoiceId)
                                                    .Select(i => (int?)i.PatientId)
                                                    .FirstOrDefaultAsync()),
                PaymentMethod = model.PaymentMethod,
                TransactionNumber = model.TransactionNumber,
                TransactionDate = model.TransactionDate,
                AmountPaid = model.AmountPaid ?? 0m,
                Remarks = model.Remarks,
                // persist the uploaded URL/path
                ProofFileName = proofUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User?.Identity?.Name ?? "system"
            };

            // Add and save to obtain the generated PaymentId, then set PaymentRefId and persist the update.
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            payment.PaymentRefId = $"PAY-{payment.PaymentId:D7}";
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment uploaded successfully.";
            return RedirectToAction("Index");
        }

        // POST: /Billing/RecordPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(int PaymentId, decimal? AmountReceived, string? Remarks)
        {
            if (PaymentId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid payment id.";
                return RedirectToAction(nameof(Index));
            }

            var payment = await _context.Payments
                                        .Include(p => p.Invoice)
                                        .FirstOrDefaultAsync(p => p.PaymentId == PaymentId);

            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }

            // validate AmountReceived when provided
            if (AmountReceived.HasValue && AmountReceived.Value < 0m)
            {
                TempData["ErrorMessage"] = "Amount received must be zero or greater.";
                return RedirectToAction(nameof(Index));
            }

            // Update amount/remarks if provided from the modal form
            if (AmountReceived.HasValue)
            {
                payment.AmountPaid = AmountReceived.Value;
            }

            if (!string.IsNullOrWhiteSpace(Remarks))
            {
                payment.Remarks = Remarks;
            }

            // mark payment verified
            payment.status = "Verified";
            // optionally set who verified
            payment.CreatedBy = User?.Identity?.Name ?? payment.CreatedBy;

            // ensure we update PaymentRefId if not present
            if (string.IsNullOrEmpty(payment.PaymentRefId))
            {
                payment.PaymentRefId = $"PAY-{payment.PaymentId:D7}";
            }

            // prepare payment history entry
            var invoice = payment.Invoice;
            decimal invoiceTotal = invoice?.TotalAmount ?? 0m;

            // compute current verified payments total for invoice (exclude this payment)
            var verifiedSum = await _context.Payments
                                    .Where(x => x.InvoiceId == payment.InvoiceId && x.status == "Verified" && x.PaymentId != payment.PaymentId)
                                    .SumAsync(x => (decimal?)x.AmountPaid) ?? 0m;

            // amount to apply is this payment's (possibly updated) amount
            decimal amountToApply = payment.AmountPaid;
            // remaining amount due after applying existing verified payments + this one
            decimal amountDue = Math.Max(0m, invoiceTotal - (verifiedSum + amountToApply));

            var ph = new PaymentHistory
            {
                PaymentId = payment.PaymentId,
                PaymentRefNumber = payment.PaymentRefId,
                InvoiceId = payment.InvoiceId,
                InvoiceRefNumber = invoice?.InvoiceRefId,
                Period = (invoice != null && invoice.Month > 0 && invoice.Year > 0) ? new DateTime(invoice.Year, invoice.Month, 1).ToString("MMM yyyy") : null,
                Month = invoice?.Month,
                Year = invoice?.Year,
                DueDate = invoice?.DueDate,
                TotalAmount = invoiceTotal,
                AmountToApply = amountToApply,
                AmountDue = amountDue,
                Remarks = payment.Remarks,
                RecordedBy = User?.Identity?.Name ?? payment.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            // persist changes
            _context.Payments.Update(payment);
            _context.PaymentHistories.Add(ph);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment recorded and marked as verified.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Billing/RejectPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int PaymentId, string? Remarks)
        {
            if (PaymentId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid payment id.";
                return RedirectToAction(nameof(Index));
            }

            var payment = await _context.Payments
                                        .Include(p => p.Invoice)
                                        .FirstOrDefaultAsync(p => p.PaymentId == PaymentId);

            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }

            // mark payment rejected
            payment.status = "Rejected";
            // store remarks (overwrite or append)
            payment.Remarks = string.IsNullOrWhiteSpace(Remarks) ? payment.Remarks : Remarks;
            // mark who performed the action
            payment.CreatedBy = User?.Identity?.Name ?? payment.CreatedBy;

            // ensure PaymentRefId exists
            if (string.IsNullOrEmpty(payment.PaymentRefId))
            {
                payment.PaymentRefId = $"PAY-{payment.PaymentId:D7}";
            }

            // prepare a payment history entry documenting the rejection
            var invoice = payment.Invoice;
            decimal invoiceTotal = invoice?.TotalAmount ?? 0m;

            // compute current verified payments total for invoice (excluding this rejected payment)
            var verifiedSum = await _context.Payments
                                    .Where(x => x.InvoiceId == payment.InvoiceId && x.status == "Verified")
                                    .SumAsync(x => (decimal?)x.AmountPaid) ?? 0m;

            var ph = new PaymentHistory
            {
                PaymentId = payment.PaymentId,
                PaymentRefNumber = payment.PaymentRefId,
                InvoiceId = payment.InvoiceId,
                InvoiceRefNumber = invoice?.InvoiceRefId,
                Period = (invoice != null && invoice.Month > 0 && invoice.Year > 0) ? new DateTime(invoice.Year, invoice.Month, 1).ToString("MMM yyyy") : null,
                Month = invoice?.Month,
                Year = invoice?.Year,
                DueDate = invoice?.DueDate,
                TotalAmount = invoiceTotal,
                AmountToApply = 0m, // rejected => nothing applied
                AmountDue = Math.Max(0, invoiceTotal - verifiedSum),
                Remarks = $"Rejected: {payment.Remarks}",
                RecordedBy = User?.Identity?.Name ?? payment.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Update(payment);
            _context.PaymentHistories.Add(ph);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
