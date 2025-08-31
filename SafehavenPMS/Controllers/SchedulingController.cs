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
using SafehavenPMS.ViewModel;
using System.Reflection.Metadata.Ecma335;

namespace SafehavenPMS.Controllers
{
    public class SchedulingController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public SchedulingController(SafehavenPMSContext context)
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
                .Include(i => i.IntakeForm)
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .AsQueryable();

            // Get waitlisted count (patients with Waitlisted status)
            ViewBag.WaitlistedCount = await _context.Patients
                .CountAsync(p => p.PatientStatus == PatientStatusEnum.Waitlisted.ToString());

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
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.PatientStatus == status);
            }

            // Apply sorting
            query = sortOrder == "ascending"
                ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                : query.OrderByDescending(p => p.CreatedAt);

            // Get total count for pagination
            ViewBag.TotalPatientCount = await query.CountAsync();

            // Apply pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            // Map to view model
            var intakeViewModels = patientList.Select(p => new IntakeViewModel
            {
                IntakeId = p.IntakeForm?.IntakeFormsId ?? 0,
                FullName = $"{p.Firstname} {p.Lastname}",
                ReferredBy = p.IntakeForm?.ReferredBy ?? "-",
                ReferredByPhoneNumber = p.PhoneNumber,
                IntakeOfficer = "-",
                IntakeDate = p.IntakeForm?.CreatedAt ?? p.CreatedAt,
                CompletedDate = "-",
                IntakeStatus = p.IntakeForm?.IntakeStatus.ToString() ?? "-"
            }).ToList();

            return View(intakeViewModels);
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
