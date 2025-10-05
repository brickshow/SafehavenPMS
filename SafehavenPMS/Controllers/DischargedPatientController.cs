using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.ViewModel;
using Microsoft.AspNetCore.Authorization;
using SafehavenPMS.Enum;


namespace SafehavenPMS.Controllers
{
    [Authorize]
    public class DischargedPatientController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public DischargedPatientController(SafehavenPMSContext context)
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
            // Query discharged patients with patient information
            var query = _context.DischargedPatients
                .Include(a => a.Patient)
                .AsQueryable();

            // Only get patients with Discharged status
            query = query.Where(a => a.Status == Enum.PatientStatusEnum.Discharged.ToString());

            // Get discharged count
            ViewBag.DischargedCount = await _context.DischargedPatients
                .CountAsync(p => p.Status == Enum.PatientStatusEnum.Discharged.ToString());

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

            // Apply status filter (optional)
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // Apply sorting
            query = sortOrder == "ascending"
                ? query.OrderBy(a => a.Patient.Firstname).ThenBy(a => a.Patient.Lastname)
                : query.OrderByDescending(a => a.DischargeDate);

            // Pagination and projection
            int totalItems = await query.CountAsync();
            ViewBag.TotalPatientCount = totalItems;
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = currentPage;

            var dischargedPatients = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            var viewModel = dischargedPatients.Select(a => new DischargedViewModel
            {
                DischargeId = a.DischargeId,
                PatientId = a.PatientId,
                Photo = a.Patient?.PhotoUrl,
                PatientName = a.Patient != null ? $"{a.Patient.Firstname} {a.Patient.Lastname}" : "Unknown",
                Reason = a.Reason,
                DischargedBy = a.CreatedBy,
                DischargeDate = a.DischargeDate,
                Status = a.Status,
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Search(string searchQuery)
        {
            // Redirect to this controller's Index so filtering/searching happens on discharged list
            return RedirectToAction("Index", new
            {
                searchQuery,
                page = 1,
                pageSize = 10,
                status = PatientStatusEnum.Discharged.ToString(),
                sortOrder = "descending"
            });
        }
        
        // POST: Patient/ReopenPatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReopenPatient(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return NotFound();

            // Set status to NewIntake when reopening a discharged patient
            patient.PatientStatus = PatientStatusEnum.NewIntake.ToString();
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient reopened to New Intake.";
            return RedirectToAction("Index", "DischargedPatient");
        }
    }
}
