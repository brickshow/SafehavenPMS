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
                   string sortOrder = null,
                  string sortBy = null)
        {
            ViewBag.SortBy = sortBy ?? "";
            ViewBag.PageSize = pageSize ?? 10;
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SearchQuery = searchQuery;

            // Do not filter by status here — return all records from DischargedPatients
            var query = _context.DischargedPatients
                .Include(a => a.Patient)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var q = searchQuery.ToLower();
                query = query.Where(a =>
                    a.Patient.Firstname.ToLower().Contains(q) ||
                    a.Patient.Lastname.ToLower().Contains(q) ||
                    a.PatientId.ToString().Contains(q));
            }

            // Apply sorting: support Name, DateAdded (patient.CreatedAt) and default DischargeDate
            var asc = string.Equals(sortOrder, "ascending", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
            {
                query = asc
                    ? query.OrderBy(a => a.Patient.Firstname).ThenBy(a => a.Patient.Lastname)
                    : query.OrderByDescending(a => a.Patient.Firstname).ThenByDescending(a => a.Patient.Lastname);
            }
            else if (string.Equals(sortBy, "DateAdded", StringComparison.OrdinalIgnoreCase))
            {
                // assumes Patient.CreatedAt exists; fallback to DischargeDate if null
                query = asc
                    ? query.OrderBy(a => a.Patient.CreatedAt)
                    : query.OrderByDescending(a => a.Patient.CreatedAt);
            }
            else
            {
                query = asc
                    ? query.OrderBy(a => a.DischargeDate)
                    : query.OrderByDescending(a => a.DischargeDate);
            }

            // Pagination and projection
            int totalItems = await query.CountAsync();
            ViewBag.TotalPatientCount = totalItems;
            int totalPages = (pageSize > 0 && pageSize.Value > 0) ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = currentPage;

            List<SafehavenPMS.Models.DischargedPatient> dischargedPatients;
            if (pageSize == 0)
            {
                dischargedPatients = await query.ToListAsync();
            }
            else
            {
                dischargedPatients = await query
                    .Skip((currentPage - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync();
            }

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

            //Remove the Patients From PatientDischarged Table
            var dischargedRecord = await _context.DischargedPatients
                .FirstOrDefaultAsync(d => d.PatientId == patientId);

            if (dischargedRecord != null)
            {
                _context.DischargedPatients.Remove(dischargedRecord);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient reopened to New Intake.";
            return RedirectToAction("Index", "DischargedPatient");
        }
    }
}
