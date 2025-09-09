using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.ViewModel;

namespace SafehavenPMS.Controllers
{
    public class TransferController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public TransferController(SafehavenPMSContext context)
        {
            _context = context;
        }
       
        public async Task<IActionResult> Index(
                   int? page = 1,
                   int? pageSize = 10,
                   string searchQuery = null,
                   string status = null,
                   string sortOrder = null)
        {
            // Query transfers with patient information
            var query = _context.PatientTransfers
                .Include(a => a.Patient)
                .AsQueryable();

            // If you want to return only Closed transfers uncomment the next line:
            query = query.Where(a => a.Patient.PatientStatus == Enum.PatientStatusEnum.Closed.ToString());

            // Get closed count (transfers with Closed status)
            ViewBag.TransfersCount = await _context.PatientTransfers
                .CountAsync(p => p.Patient.PatientStatus == Enum.PatientStatusEnum.Closed.ToString());

            // Pass current filters/sorting to view
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(a =>
                    a.Patient.Firstname.ToLower().Contains(searchQuery) ||
                    a.Patient.Lastname.ToLower().Contains(searchQuery) ||
                    a.PatientId.ToString().Contains(searchQuery));
            }

            // Apply status filter (optional - will further restrict results)
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // Apply sorting
            query = sortOrder == "ascending"
                ? query.OrderBy(a => a.Patient.Firstname).ThenBy(a => a.Patient.Lastname)
                : query.OrderByDescending(a => a.CreatedAt);

            // Pagination and projection
            int totalItems = await query.CountAsync();
            ViewBag.TotalPatientCount = totalItems;
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = currentPage;

            var transfers = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            var viewModel = transfers.Select(a => new TransferViewModel
            {
                TransferId = a.TransferId,
                PatientId = a.PatientId,
                FromFacility = a.FromFacility,
                TransferDate = a.TransferDate,
                PatientName = a.Patient != null ? $"{a.Patient.Firstname} {a.Patient.Lastname}" : "-",
                ToFacility = a.ToFacility,
                ProgramType = a.ProgramType,
                Reason = a.Reason,
                CreatedBy = a.CreatedBy,
                CreatedAt = a.CreatedAt,
                Status = a.Status,
                Photo = a.Patient?.PhotoUrl
            }).ToList();

            return View(viewModel);
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
                sortOrder = ViewBag.SortOrder
            });
        }
    }
}