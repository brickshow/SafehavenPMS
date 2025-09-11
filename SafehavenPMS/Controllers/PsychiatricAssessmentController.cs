using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.ViewModel;

namespace safehavenpms.Controllers
{
    public class PsychiatricAssessmentController : Controller
    {
        private readonly SafehavenPMSContext _context;
        public PsychiatricAssessmentController(SafehavenPMSContext context)
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
            var query = _context.PsychiatricAssessments
                        .Include(pt => pt.Patient)
                        .AsQueryable();

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
                    // ensure patient is not null before accessing its names, and guard each name with ?? ""
                    (p.Patient != null &&
                        (
                            (p.Patient.Firstname ?? "").ToLower().Contains(searchQuery) ||
                            (p.Patient.Lastname ?? "").ToLower().Contains(searchQuery)
                        )
                    )
                    // allow matching by PatientId as well
                    || p.PatientId.ToString().Contains(searchQuery)
                );
            }

            // Apply status filter (default = All)
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Patient.PatientStatus.ToString() == status);
            }

            //Apply sorting
            if (sortOrder == null)
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(p => p.Patient.Firstname).ThenBy(p => p.Patient.Lastname)
                    : query.OrderByDescending(p => p.Patient.Firstname).ThenByDescending(p => p.Patient.Lastname);
            }

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();


            // Project to PsychiatricAssessmentViewModel
            var psychiatricViewModels = patientList
                                   .Where(p => p.Patient.PatientStatus == PatientStatusEnum.Admitted.ToString())
                                   .Select(p => new PsychiatricAssessmentViewModel
                                   {
                                       // If you have a dedicated PK on the transfer entity use that instead of PatientId
                                       PsychiatricAssessmentId = p.PsychiatricAssessmentId,
                                       PatientId = p.PatientId,
                                       FullName = $"{p.Patient.Firstname} {p.Patient.Lastname}",
                                       Type = p.Type, // Assuming type is always "Initial" for pending assessments
                                       Date = p.CreatedAt,
                                       CompletedDate = null, // Pending assessments are not completed
                                       Status = "Pending"
                                   }).ToList() ?? new List<PsychiatricAssessmentViewModel>();

            //Return Total number of new referral
            var Pending = await _context.Patients
                                    .Where(p => p.PatientStatus == PatientStatusEnum.Admitted.ToString())
                                    .ToListAsync();

            ViewBag.Pending = Pending.Count();
            return View(psychiatricViewModels);
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
        public async Task<IActionResult> PsychiatricAssessmentForm(int id)
        {
            // Try to load assessment by its PK or as fallback by patient id
            var assessment = await _context.PsychiatricAssessments
                                           .Include(a => a.Patient)
                                           .FirstOrDefaultAsync(a => a.PatientId == id || a.PatientId == id);

            // If assessment not found, try to load patient directly (id might be a patientId)
            var patient = assessment?.Patient;
            if (patient == null)
            {
                patient = await _context.Patients.FindAsync(id);
            }

            if (assessment == null && patient == null)
                return NotFound();

            // calculate age as number of years (safe if DateOfBirth is null)
            int? age = null;
            if (patient?.DateOfBirth != null)
            {
                var dob = patient.DateOfBirth;
                var today = DateTime.Today;
                var years = today.Year - dob.Year;
                if (dob.Date > today.AddYears(-years)) years--;
                age = years;
            }

            var vm = new PsychiatricAssessmentViewModel
            {
                PsychiatricAssessmentId = assessment?.PsychiatricAssessmentId ?? 0,
                PatientId = patient?.PatientId ?? assessment?.PatientId ?? 0,
                FullName = $"{(patient?.Firstname ?? "").Trim()} {(patient?.Lastname ?? "").Trim()}".Trim(),
                Age = age,
                Sex = patient?.Sex,
                Occupation = patient?.Occupation,
                Address = patient?.Address,
                Type = assessment?.Type,
                Date = assessment?.CreatedAt,
                Time = assessment?.CreatedAt,
                CompletedDate = null,
                Status = patient?.PatientStatus,

                //Tab content
                ChiefComplaint = assessment?.ChiefComplaint,
                HistoryOfPresentIllness = assessment?.HistoryOfPresentIllness,
                PersonalAndFamilyHistory = assessment?.PersonalAndFamilyHistory

                //populate tab view models if assessment exists
            };

            return View("PsychiatricAssessmentForm", vm);
        }

        //Saving Chief complaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavingChiefComplaint(PsychiatricAssessmentViewModel model)
        {
            int PatientId = model.PatientId;
            string ChiefComplaint = model.ChiefComplaint;

            //Find assessment by patient id
            var assessment = await _context.PsychiatricAssessments.FirstOrDefaultAsync(a => a.PatientId == PatientId);
            if (assessment != null)
            {
                assessment.ChiefComplaint = ChiefComplaint;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Chief Complaint saved successfully.";
            return RedirectToAction("PsychiatricAssessmentForm", new { id = PatientId });
        }

        //Saving History of Present Illness
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavingHistoryOfPresent(PsychiatricAssessmentViewModel model)
        {
            int PatientId = model.PatientId;
            string HistoryOfPresentIllness = model.HistoryOfPresentIllness;

            //Find assessment by patient id
            var assessment = await _context.PsychiatricAssessments.FirstOrDefaultAsync(a => a.PatientId == PatientId);
            if (assessment != null)
            {
                assessment.HistoryOfPresentIllness = HistoryOfPresentIllness;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "History of Present Illness saved successfully.";
            return RedirectToAction("PsychiatricAssessmentForm", new { id = PatientId });
        }

        //Saving Personal and family
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavingPersonalAndFamily(PsychiatricAssessmentViewModel model)
        {
            //Find assessment by patient id
            var assessment = await _context.PsychiatricAssessments.FirstOrDefaultAsync(a => a.PatientId == model.PatientId);
            if (assessment != null)
            {
                assessment.PersonalAndFamilyHistory = model.PersonalAndFamilyHistory;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Personal and Family History saved successfully.";
            return RedirectToAction("PsychiatricAssessmentForm", new { id = model.PatientId });
        }
    }
}