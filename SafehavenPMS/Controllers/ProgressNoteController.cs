using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // <--- added
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SafehavenPMS.Controllers
{
    public class ProgressNoteController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public ProgressNoteController(SafehavenPMSContext context)
        {
            _context = context;
        }

        // Show interventions and (when patientId provided) medication orders for a patient.
        public async Task<IActionResult> Index(
            int? patientId = null,
            int? page = 1,
            int? pageSize = 10,
            string searchQuery = null,
            string status = null,
            string sortOrder = null)
        {
            // Base query with related data
            var query = _context.Interventions
                .Include(i => i.Patient)
                .Include(i => i.Problem)
                .Include(i => i.ServiceType)
                .Include(i => i.ServiceModality)
                .AsQueryable();

            // If patientId specified, restrict interventions to that patient
            if (patientId.HasValue)
            {
                query = query.Where(i => i.PatientId == patientId.Value);
                ViewBag.PatientId = patientId.Value;
            }

            // Counts and status breakdown (respect current patient filter if provided)
            ViewBag.TotalCount = await query.CountAsync();
            ViewBag.StatusCounts = await query
                .GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Current filters/sorting
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // Search filter (search patient name, description, noted by, or id)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var sq = searchQuery.ToLower();
                query = query.Where(i =>
                    (i.Description ?? string.Empty).ToLower().Contains(sq) ||
                    (i.NotedBy ?? string.Empty).ToLower().Contains(sq) ||
                    (i.Patient.Firstname ?? string.Empty).ToLower().Contains(sq) ||
                    (i.Patient.Lastname ?? string.Empty).ToLower().Contains(sq) ||
                    i.InterventionId.ToString().Contains(sq));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => i.Status == status);
            }

            // Sorting (default by DateAdded desc)
            if (sortOrder == null)
            {
                query = query.OrderByDescending(i => i.DateAdded);
            }
            else
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(i => i.DateAdded)
                    : query.OrderByDescending(i => i.DateAdded);
            }

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var interventions = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            // ensure view header uses same name the view expects
            ViewBag.TotalPatientCount = ViewBag.TotalCount;

            // Also fetch medication orders for the specified patient (if patientId provided).
            if (patientId.HasValue)
            {
                var meds = await _context.MedicationOrders
                    .Include(m => m.Medicine)
                    .Include(m => m.Patient) // include Patient so view can show patient name
                    .Where(m => m.PatientId == patientId.Value)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();

                ViewBag.MedicationOrders = meds;
            }
            else
            {
                // provide a typed empty enumerable to avoid casting/null issues in the view
                ViewBag.MedicationOrders = Enumerable.Empty<SafehavenPMS.Models.MedicationOrder>();
            }

            return View(interventions);
        }

        public async Task<IActionResult> ProgressNoteLists(int? patientId = null, int? selectedId = null)
        {
            // Base query with related data
            var query = _context.Interventions
                .Include(i => i.Patient)
                .Include(i => i.Problem)
                .Include(i => i.ServiceType)
                .Include(i => i.ServiceModality)
                .AsQueryable();

            if (patientId.HasValue)
            {
                query = query.Where(i => i.PatientId == patientId.Value);
                ViewBag.PatientId = patientId.Value;

                var patient = await _context.Patients.FindAsync(patientId.Value);
                ViewBag.PatientName = patient != null ? $"{patient.Firstname} {patient.Lastname}" : null;
            }

            var interventions = await query
                .OrderByDescending(i => i.DateAdded)
                .ToListAsync();

            // choose selected intervention (if provided) or default to first
            SafehavenPMS.Models.Intervention selected = null;
            if (selectedId.HasValue)
            {
                selected = interventions.FirstOrDefault(i => i.InterventionId == selectedId.Value)
                           ?? await _context.Interventions
                                .Include(i => i.Patient)
                                .Include(i => i.Problem)
                                .Include(i => i.ServiceType)
                                .Include(i => i.ServiceModality)
                                .FirstOrDefaultAsync(i => i.InterventionId == selectedId.Value);
            }

            if (selected == null && interventions.Any())
            {
                selected = interventions.First();
            }

            ViewBag.SelectedIntervention = selected;
            ViewBag.SelectedInterventionId = selected?.InterventionId;

            return View(interventions);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? patientId = null, int? interventionId = null)
        {
            var intervention = await _context.Interventions
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.InterventionId == interventionId);

            var model = new ProgressNoteCreateViewModel
            {
                InterventionId = interventionId,
                PatientId = patientId
            };
            return View(model);
        }

        // NEW: Save progress note
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProgressNoteCreateViewModel model)
        {
            // Try derive patientId from intervention if not provided
            if (model.InterventionId == null)
            {
                ModelState.AddModelError("InterventionId", "Intervention is required.");
            }

            if (!ModelState.IsValid)
            {
                // Return the view with validation errors
                return View(model);
            }

            var note = new ProgressNote
            {
                PatientId = model.PatientId,
                InterventionId = model.InterventionId,
                CreatedAt = DateTime.UtcNow,
                Subjective = string.IsNullOrWhiteSpace(model.Subjective) ? null : model.Subjective,
                Objective = string.IsNullOrWhiteSpace(model.Objective) ? null : model.Objective,
                Assessment = string.IsNullOrWhiteSpace(model.Assessment) ? null : model.Assessment,
                Plan = string.IsNullOrWhiteSpace(model.Plan) ? null : model.Plan
            };

            // Build SoapRaw like S:...|O:...|A:...|P:...
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(model.Subjective)) parts.Add($"S:{model.Subjective}");
            if (!string.IsNullOrWhiteSpace(model.Objective)) parts.Add($"O:{model.Objective}");
            if (!string.IsNullOrWhiteSpace(model.Assessment)) parts.Add($"A:{model.Assessment}");
            if (!string.IsNullOrWhiteSpace(model.Plan)) parts.Add($"P:{model.Plan}");
            note.SoapRaw = parts.Any() ? string.Join("|", parts) : null;

            _context.ProgressNotes.Add(note);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Progress note saved.";

            return RedirectToAction("Index", "PatientProfile", new { id = model.PatientId });
        }
    }
}