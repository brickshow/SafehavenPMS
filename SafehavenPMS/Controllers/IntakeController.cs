using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace SafehavenPMS.Controllers
{
    public class IntakeController : Controller
    {
        private readonly SafehavenPMSContext _context;
        public IntakeController(SafehavenPMSContext context)
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
            var query = _context.Patients
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .AsQueryable();

            // Counts for each status
            ViewBag.TotalPatientCount = await _context.Patients.CountAsync();
            ViewBag.WaitlistedCount = await _context.Patients.CountAsync(p => p.PatientStatus == Enum.PatientStatusEnum.Waitlisted.ToString());
            ViewBag.PendingAssessmentCount = await _context.Patients.CountAsync(p => p.PatientStatus == Enum.PatientStatusEnum.PendingAssessment.ToString());
            ViewBag.PendingApprovalCount = await _context.Patients.CountAsync(p => p.PatientStatus == Enum.PatientStatusEnum.PendingApproval.ToString());
            ViewBag.ActiveCount = await _context.Patients.CountAsync(p => p.PatientStatus == "Active");
            ViewBag.InactiveCount = await _context.Patients.CountAsync(p => p.PatientStatus == "Inactive");
            ViewBag.AdmittedCount = await _context.Patients.CountAsync(p => p.PatientStatus == "Admitted");

            // Pass current filters/sorting to view
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // 🔎 Apply search filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery));
            }

            // ✅ Apply status filter (default = All)
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.PatientStatus == status);
            }

            // 🔃 Apply sorting
            if (sortOrder == null)
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                    : query.OrderByDescending(p => p.Firstname).ThenByDescending(p => p.Lastname);
            }

            // 📄 Pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            return View(patientList);
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

        [HttpGet]
        public IActionResult SortBy(string sortOrder)
        {
            return RedirectToAction("Index", new
            {
                sortOrder,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                searchQuery = ViewBag.SearchQuery,
                status = ViewBag.Status
            });
        }
    }
}