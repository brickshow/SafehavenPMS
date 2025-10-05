using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // <--- added
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Controllers
{
    [Authorize]
    public partial class ProgressNoteController : Controller
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
            string sortOrder = null,
            string sortBy = null)
        {
            // Base query with related data
            var query = _context.Interventions
                .Include(i => i.Patient)
                .Include(i => i.Problem)
                .Include(i => i.ServiceType)
                .Include(i => i.ServiceModality)
                .AsQueryable();

            // --- NEW: restrict to patients assigned to the current user ---
            // Restrict results to interventions whose patient is assigned to the logged-in clinical staff (unless Admin).
            // Follows the same logic used in PatientController.Index to resolve the current user's ClinicalStaff association.
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                var appUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                if (appUser != null && !string.Equals(appUser.Role ?? string.Empty, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    if (appUser.ClinicalStaffID.HasValue)
                    {
                        var staffId = appUser.ClinicalStaffID.Value;
                        query = query.Where(i => i.Patient != null && i.Patient.ClinicalStaffPatients.Any(csp => csp.ClinicalStaffId == staffId));
                    }
                    else if (!string.IsNullOrWhiteSpace(appUser.Email))
                    {
                        var cs = await _context.ClinicalStaffs.AsNoTracking().FirstOrDefaultAsync(c => c.Email.ToLower() == appUser.Email.Trim().ToLower());
                        if (cs != null)
                        {
                            query = query.Where(i => i.Patient != null && i.Patient.ClinicalStaffPatients.Any(csp => csp.ClinicalStaffId == cs.ClinicalStaffID));
                        }
                        else
                        {
                            // Not linked to a clinical staff -> no results
                            query = query.Where(i => false);
                        }
                    }
                    else
                    {
                        query = query.Where(i => false);
                    }
                }
            }
            // --- end new code ---

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
            ViewBag.SortBy = sortBy ?? "";
 
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

            // Sorting: support sortBy (Title or DateAdded) + sortOrder (ascending/descending)
            if (!string.IsNullOrWhiteSpace(sortBy) && sortBy.Equals("Title", StringComparison.OrdinalIgnoreCase))
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(i => i.ServiceModality.ServiceName)
                    : query.OrderByDescending(i => i.ServiceModality.ServiceName);
            }
            else // default DateAdded
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(i => i.DateAdded)
                    : query.OrderByDescending(i => i.DateAdded);
            }
 
            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page ?? 1;
 
             int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
             ViewBag.CurrentPage = currentPage;

            var interventions = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            // Group interventions by patient for the Index view
            var groupedByPatient = interventions
                .GroupBy(i => new
                {
                    PatientId = i.Patient?.PatientId ?? 0,
                    PatientName = i.Patient != null
                        ? (string.IsNullOrWhiteSpace(i.Patient.Firstname) && string.IsNullOrWhiteSpace(i.Patient.Lastname)
                            ? (i.Patient.PatientId.ToString() ?? "Patient")
                            : (i.Patient.Firstname + " " + i.Patient.Lastname).Trim())
                        : "No Patient"
                })
                .Select(g => new
                {
                    PatientId = g.Key.PatientId,    
                    PatientName = g.Key.PatientName,
                    Interventions = g.Select(x => new
                    {
                        x.InterventionId,
                        Title = x.ServiceModality?.ServiceName ?? "Intervention",
                        Description = x.Description ?? "",
                        Status = x.Status ?? "Active",
                        Clinician = x.NotedBy ?? "",
                        DateAdded = x.DateAdded
                    }).OrderByDescending(ii => ii.DateAdded).ToList()
                })
                .OrderBy(p => p.PatientName)
                .ToList();

            ViewBag.GroupedInterventions = groupedByPatient;

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
            try
            {
                // Load all interventions (no patient filter)
                var interventions = await _context.Interventions
                    .Include(i => i.Patient)
                    .Include(i => i.ServiceModality)
                    .OrderByDescending(i => i.DateAdded)
                    .ToListAsync();

                // Get progress notes for the loaded interventions (match by InterventionId)
                var interventionIds = interventions.Select(i => i.InterventionId).ToList();
                var progressNotes = interventionIds.Any()
                    ? await _context.ProgressNotes
                        .Where(pn => pn.InterventionId.HasValue && interventionIds.Contains(pn.InterventionId.Value))
                        .ToListAsync()
                    : new List<ProgressNote>();

                // Group and map notes
                var notesByIntervention = progressNotes
                    .GroupBy(n => n.InterventionId)
                    .ToDictionary(
                        g => g.Key ?? 0,
                        g => g.OrderByDescending(n => n.CreatedAt)
                              .Select(n => new ProgressNoteSummaryViewModel
                              {
                                  ProgressNoteId = n.ProgressNoteId,
                                  CreatedAt = n.CreatedAt,
                                  Clinician = n.Clinician ?? "-",
                                  SoapRaw = n.SoapRaw ?? "",
                                  Subjective = n.Subjective ?? "",
                                  Objective = n.Objective ?? "",
                                  Assessment = n.Assessment ?? "",
                                  Plan = n.Plan ?? ""
                              })
                              .ToList()
                    );

                // Map interventions -> summary VM (attach mapped notes)
                var interventionSummaries = interventions
                    .Select(i =>
                    {
                        var noteList = notesByIntervention.ContainsKey(i.InterventionId)
                            ? notesByIntervention[i.InterventionId]
                            : new List<ProgressNoteSummaryViewModel>();

                        return new InterventionSummaryViewModel
                        {
                            InterventionId = i.InterventionId,
                            Title = i.ServiceModality?.ServiceName ?? "Intervention",
                            Description = i.Description ?? "",
                            Frequency = i.DurationFrequency,
                            Status = i.Status ?? "Active",
                            Clinician = i.NotedBy ?? "",
                            LastNoteDate = noteList.Any() ? noteList.First().CreatedAt : i.DateAdded,
                            LastNoteDisplay = noteList.Any() ? noteList.First().CreatedAt.ToString("MMM dd, yyyy") : (i.DateAdded?.ToString("MMM dd, yyyy") ?? ""),
                            ProgressNotes = noteList
                        };
                    })
                    .ToList();

                // choose selected intervention
                var selected = selectedId.HasValue
                    ? interventionSummaries.FirstOrDefault(v => v.InterventionId == selectedId.Value)
                    : interventionSummaries.FirstOrDefault(v => v.ProgressNotes != null && v.ProgressNotes.Any()) ?? interventionSummaries.FirstOrDefault();

                var model = new SafehavenPMS.ViewModel.PatientProgressNotesTabViewModel
                {
                    PatientId = null,
                    Interventions = interventionSummaries,
                    SelectedInterventionId = selected?.InterventionId,
                    InterventionFilter = "All"
                };

                ViewBag.SelectedIntervention = interventions.FirstOrDefault(i => i.InterventionId == selected?.InterventionId);
                ViewBag.SelectedInterventionId = selected?.InterventionId;

                // Set model.PatientId from the actual selected Intervention entity (so Create links work)
                if (ViewBag.SelectedIntervention != null)
                {
                    model.PatientId = ((SafehavenPMS.Models.Intervention)ViewBag.SelectedIntervention).PatientId;
                }

                return View("ProgressNoteLIsts", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.UtcNow:O}] ERROR in ProgressNoteLists: {ex.GetType().FullName}: {ex.Message}");
                Console.WriteLine(ex.ToString());
                throw;
            }
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

            return RedirectToAction("ProgressNoteLists", "ProgressNote", new { id = model.PatientId });
        }

        // GET: ProgressNote/EditSoap/5
        public async Task<IActionResult> EditSoap(int id)
        {
            var note = await _context.ProgressNotes.FindAsync(id);
            if (note == null) return NotFound();

            var vm = new ProgressNoteEditViewModel
            {
                ProgressNoteId = note.ProgressNoteId,
                PatientId = note.PatientId ?? 0,
                InterventionId = note.InterventionId,
                SoapRaw = note.SoapRaw,
                Subjective = note.Subjective,
                Objective = note.Objective,
                Assessment = note.Assessment,
                Plan = note.Plan,
                CreatedAt = note.CreatedAt
            };

            // If SoapRaw is present, attempt to parse into S/O/A/P for the split editors
            if (!string.IsNullOrWhiteSpace(note.SoapRaw))
            {
                var parts = note.SoapRaw.Split('|');
                foreach (var part in parts)
                {
                    var colonIndex = part.IndexOf(':');
                    if (colonIndex <= 0) continue;
                    var label = part.Substring(0, colonIndex).Trim();
                    var content = part.Substring(colonIndex + 1).Trim();
                    switch (label.ToUpperInvariant())
                    {
                        case "S":
                        case "SUBJECTIVE":
                            vm.Subjective = content;
                            break;
                        case "O":
                        case "OBJECTIVE":
                            vm.Objective = content;
                            break;
                        case "A":
                        case "ASSESSMENT":
                            vm.Assessment = content;
                            break;
                        case "P":
                        case "PLAN":
                            vm.Plan = content;
                            break;
                    }
                }
            }

            return View(vm);
        }

        // POST: ProgressNote/EditSoap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSoap(ProgressNoteEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
 
            var note = await _context.ProgressNotes.FindAsync(vm.ProgressNoteId);
            if (note == null) return NotFound();
 
            // If user provided a SoapRaw, prefer that. Otherwise compose from fields.
            if (!string.IsNullOrWhiteSpace(vm.SoapRaw))
            {
                note.SoapRaw = vm.SoapRaw.Trim();
            }
            else
            {
                // Build SoapRaw only from non-empty parts
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(vm.Subjective)) parts.Add("S:" + vm.Subjective.Trim());
                if (!string.IsNullOrWhiteSpace(vm.Objective)) parts.Add("O:" + vm.Objective.Trim());
                if (!string.IsNullOrWhiteSpace(vm.Assessment)) parts.Add("A:" + vm.Assessment.Trim());
                if (!string.IsNullOrWhiteSpace(vm.Plan)) parts.Add("P:" + vm.Plan.Trim());

                note.SoapRaw = parts.Count > 0 ? string.Join("|", parts) : null;
            }
 
            // Keep separate stored fields in sync (optional but helpful)
            note.Subjective = string.IsNullOrWhiteSpace(vm.Subjective) ? null : vm.Subjective.Trim();
            note.Objective = string.IsNullOrWhiteSpace(vm.Objective) ? null : vm.Objective.Trim();
            note.Assessment = string.IsNullOrWhiteSpace(vm.Assessment) ? null : vm.Assessment.Trim();
            note.Plan = string.IsNullOrWhiteSpace(vm.Plan) ? null : vm.Plan.Trim();
 
            // Debug: show resulting SoapRaw before save
            Console.WriteLine($"[EditSoap POST] Saving note {note.ProgressNoteId}: SoapRawLength={(note.SoapRaw?.Length ?? 0)} SoapRawPreview={(note.SoapRaw != null && note.SoapRaw.Length > 200 ? note.SoapRaw.Substring(0,200) + "..." : note.SoapRaw ?? "<null>")}");
 
            _context.Update(note);
            await _context.SaveChangesAsync();

            // Re-read saved entity to verify persistence and log final values
            var saved = await _context.ProgressNotes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProgressNoteId == note.ProgressNoteId);
            Console.WriteLine($"[EditSoap POST] DB verify: ProgressNoteId={(saved?.ProgressNoteId.ToString() ?? "<null>")}, PatientId={(saved?.PatientId?.ToString() ?? "<null>")}, InterventionId={(saved?.InterventionId?.ToString() ?? "<null>")}, SoapRawLength={(saved?.SoapRaw?.Length ?? 0)}, SoapRawPreview={(saved?.SoapRaw ?? "<null>")}");
 
            // Redirect to the list view and show the updated data; select the intervention so the UI shows the updated Soap snippet
            return RedirectToAction(nameof(ProgressNoteLists), new { patientId = saved?.PatientId, selectedId = saved?.InterventionId });
        }
    }
}