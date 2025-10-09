using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.Services.Billing;
using SafehavenPMS.ViewModel.Canteen;


namespace SafehavenPMS.Controllers
{
    [Authorize]
    public class CanteenController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly IBillingService? _billing;

        public CanteenController(SafehavenPMSContext context, IBillingService? billing = null)
        {
            _context = context;
            _billing = billing;
        }

        // GET: Canteen (list with search / filter / pagination patterned after PatientController)
        public async Task<IActionResult> Index(
            int? page = 1,
            int? pageSize = 10,
            string searchQuery = null,
            string status = null,
            string sortOrder = null,
            string sortBy = null)
        {
            var query = _context.CanteenPurchases
                .Include(p => p.Patient)
                .AsQueryable();

            // Total count (filtered later by status if applied)
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy ?? "";
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;
            ViewBag.PageSize = pageSize ?? 10;

            // Search
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var s = searchQuery.Trim().ToLower();
                query = query.Where(p =>
                    p.Patient.Firstname.ToLower().Contains(s) ||
                    p.Patient.Lastname.ToLower().Contains(s) ||
                    p.PatientId.ToString().Contains(s) ||
                    p.ItemDescription.ToLower().Contains(s));
            }

            // Status filter (Pending Review / Approved / Rejected)
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Status == status);
            }

            // Count AFTER filters
            ViewBag.TotalPurchaseCount = await query.CountAsync();

            // Sorting (follow patient pattern)
            if (string.IsNullOrEmpty(sortBy))
            {
                // default newest first
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                bool asc = ViewBag.SortOrder == "ascending";
                switch (sortBy)
                {
                    case "Name":
                        query = asc
                            ? query.OrderBy(p => p.Patient.Firstname).ThenBy(p => p.Patient.Lastname)
                            : query.OrderByDescending(p => p.Patient.Firstname).ThenByDescending(p => p.Patient.Lastname);
                        break;
                    case "DateAdded":
                    case "Date":
                        query = asc
                            ? query.OrderBy(p => p.CreatedAt)
                            : query.OrderByDescending(p => p.CreatedAt);
                        break;
                    case "Amount":
                        query = asc
                            ? query.OrderBy(p => p.Quantity * p.UnitPrice)
                            : query.OrderByDescending(p => p.Quantity * p.UnitPrice);
                        break;
                    case "Status":
                        query = asc
                            ? query.OrderBy(p => p.Status).ThenByDescending(p => p.CreatedAt)
                            : query.OrderByDescending(p => p.Status).ThenByDescending(p => p.CreatedAt);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                }
            }

            // Pagination
            int totalItems = await query.CountAsync();
            int effectivePageSize = (pageSize ?? 10);
            int totalPages = effectivePageSize > 0 ? (int)Math.Ceiling((double)totalItems / effectivePageSize) : 1;
            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = currentPage;

            var list = await query
                .Skip(effectivePageSize > 0 ? (currentPage - 1) * effectivePageSize : 0)
                .Take(effectivePageSize > 0 ? effectivePageSize : totalItems)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Index", new
            {
                searchQuery,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                status = ViewBag.Status,
                sortOrder = ViewBag.SortOrder,
                sortBy = ViewBag.SortBy
            });
        }

        [HttpGet]
        public IActionResult SortBy(string sortBy, string sortOrder)
        {
            // Toggle handled from view; we just redirect
            return RedirectToAction("Index", new
            {
                sortBy,
                sortOrder,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                searchQuery = ViewBag.SearchQuery,
                status = ViewBag.Status
            });
        }

        // GET: Create Purchase (Modal)
        [Authorize(Roles = "Cashier,Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SafehavenPMS.ViewModel.Canteen.CreateCanteenPurchaseViewModel());
        }

        // POST: Add Purchase (Modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cashier,Admin")]
        public async Task<IActionResult> Create(CreateCanteenPurchaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                Console.WriteLine($"ModelState error for '{state.Key}': {error.ErrorMessage}");
                }
            }
            TempData["Error"] = "Please correct the errors.";
            return RedirectToAction(nameof(Index));
            }

            var patient = await _context.Patients.FindAsync(model.PatientId);
            if (patient == null)
            {
            TempData["Error"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
            }

            var purchase = new CanteenPurchase
            {
            PatientId = model.PatientId,
            ItemDescription = model.ItemDescription.Trim(),
            Quantity = model.Quantity,
            UnitPrice = model.UnitPrice,
            Status = "Pending Review",
            CreatedBy = User?.Identity?.Name ?? "System"
            };

            _context.CanteenPurchases.Add(purchase);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Canteen purchase added successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task EnsureBillableForApprovedPurchase(CanteenPurchase purchase)
        {
            if (purchase == null || purchase.Status != "Approved") return;

            // // If BillingService registered use it (handles idempotency)
            // if (_billing != null)
            // {
            //     var total = purchase.Quantity * purchase.UnitPrice;
            //     await _billing.AddBillableForCanteenPurchase(purchase.PatientId, purchase.Id, total, purchase.ItemDescription);
            //     return;
            // }

            // Fallback (in case service not injected)
            bool exists = await _context.Billables
                .AnyAsync(b => b.ReferenceId == purchase.Id && b.Category == "Canteen");

            if (!exists)
            {
                var total = purchase.Quantity * purchase.UnitPrice;

                // Get next BillableId (approx) to build Bill code (BILL-000000)
                int lastId = await _context.Billables
                    .OrderByDescending(b => b.BillableId)
                    .Select(b => b.BillableId)
                    .FirstOrDefaultAsync();

                int nextId = lastId + 1;
                string billCode = $"BILL-{nextId:000000}";

                _context.Billables.Add(new Billable
                {
                    PatientId = purchase.PatientId,
                    Category = "Canteen",
                    DateAdded = DateTime.UtcNow,
                    Description =purchase.ItemDescription,
                    Quantity = purchase.Quantity,
                    UnitPrice = purchase.UnitPrice,
                    Amount = total,
                    ReferenceId = purchase.Id,
                    ReferenceType = billCode, // was CAN-xxxxx; now BILL-xxxxxx using next billable sequence
                    CreatedBy = User?.Identity?.Name ?? "System"
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task RemoveBillableForPurchase(int purchaseId)
        {
            var billables = await _context.Billables
                .Where(b => b.ReferenceType == "CanteenPurchase" && b.ReferenceId == purchaseId)
                .ToListAsync();
            if (billables.Any())
            {
                _context.Billables.RemoveRange(billables);
                await _context.SaveChangesAsync();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach,Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var purchase = await _context.CanteenPurchases.FindAsync(id);
            if (purchase == null)
            {
                TempData["Error"] = "Purchase not found.";
                return RedirectToAction(nameof(Index));
            }
            if (purchase.Status == "Approved")
            {
                TempData["Error"] = "Already approved.";
                return RedirectToAction(nameof(Index));
            }

            purchase.Status = "Approved";
            purchase.ApprovedAt = DateTime.UtcNow;
            purchase.ApprovedBy = User?.Identity?.Name ?? "Coach";
            purchase.RejectedAt = null;
            purchase.RejectedBy = null;

            _context.CanteenPurchases.Update(purchase);
            await _context.SaveChangesAsync();

            await EnsureBillableForApprovedPurchase(purchase);

            TempData["SuccessMessage"] = "Purchase approved & billed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach,Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var purchase = await _context.CanteenPurchases.FindAsync(id);
            if (purchase == null)
            {
                TempData["Error"] = "Purchase not found.";
                return RedirectToAction(nameof(Index));
            }
            if (purchase.Status == "Rejected")
            {
                TempData["Error"] = "Already rejected.";
                return RedirectToAction(nameof(Index));
            }

            purchase.Status = "Rejected";
            purchase.RejectedAt = DateTime.UtcNow;
            purchase.RejectedBy = User?.Identity?.Name ?? "Coach";
            purchase.ApprovedAt = null;
            purchase.ApprovedBy = null;

            _context.CanteenPurchases.Update(purchase);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase rejected.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach,Admin")]
        public async Task<IActionResult> RevokeApproval(int id)
        {
            var purchase = await _context.CanteenPurchases.FindAsync(id);
            if (purchase == null)
            {
                TempData["Error"] = "Purchase not found.";
                return RedirectToAction(nameof(Index));
            }
            if (purchase.Status != "Approved")
            {
                TempData["Error"] = "Only approved purchases can be revoked.";
                return RedirectToAction(nameof(Index));
            }

            purchase.Status = "Pending Review";
            purchase.ApprovedAt = null;
            purchase.ApprovedBy = null;

            _context.CanteenPurchases.Update(purchase);
            await _context.SaveChangesAsync();

            await RemoveBillableForPurchase(purchase.Id);

            TempData["SuccessMessage"] = "Approval revoked & billing entry removed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach,Admin")]
        public async Task<IActionResult> UndoRejection(int id)
        {
            var purchase = await _context.CanteenPurchases.FindAsync(id);
            if (purchase == null)
            {
                TempData["Error"] = "Purchase not found.";
                return RedirectToAction(nameof(Index));
            }
            if (purchase.Status != "Rejected")
            {
                TempData["Error"] = "Only rejected purchases can be undone.";
                return RedirectToAction(nameof(Index));
            }

            purchase.Status = "Pending Review";
            purchase.RejectedAt = null;
            purchase.RejectedBy = null;

            _context.CanteenPurchases.Update(purchase);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Rejection undone.";
            return RedirectToAction(nameof(Index));
        }

        // Patient search (autocomplete for Create page)
        [HttpGet]
        public async Task<IActionResult> SearchPatients(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(Array.Empty<object>());

            q = q.Trim().ToLower();
            var matches = await _context.Patients
                .Where(p =>
                    p.Firstname.ToLower().Contains(q) ||
                    p.Lastname.ToLower().Contains(q) ||
                    p.PatientRefId.ToString().Contains(q))
                .OrderBy(p => p.Firstname)
                .Take(15)
                .Select(p => new
                {
                    id = p.PatientId,
                    text = p.Firstname + " " + p.Lastname + " (ID:" + p.PatientRefId + ")"
                })
                .ToListAsync();

            return Json(matches);
        }
    }
}
