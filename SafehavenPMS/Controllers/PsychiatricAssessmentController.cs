using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.ViewModel;
using SafehavenPMS.Models;
using System.Linq;
using System;
using safehavenpms.Enum;
using Microsoft.AspNetCore.Authorization;
using SafehavenPMS.Services; // <-- added


namespace safehavenpms.Controllers
{
[Authorize]
    public partial class PsychiatricAssessmentController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly ActivityLogService _activityService; // <-- added
        private static string PatientFullName(Patient p) => p == null ? "" : $"{p.Firstname} {p.Lastname}"; // <-- added

        public PsychiatricAssessmentController(SafehavenPMSContext context, ActivityLogService activityService) // <-- modified
        {
            _context = context;
            _activityService = activityService; // <-- added
        }

        public async Task<IActionResult> Index(
    int? page = 1,
    int? pageSize = 10,
    string searchQuery = null,
    string status = null,
    string sortOrder = null,
    string sortBy = null)
{
    var admitted = PatientStatusEnum.Admitted.ToString();

    var query = _context.Patients
        .Include(pt => pt.PsychiatricAssessments)
        .Where(p => p.PatientStatus == admitted || p.PatientStatus == PatientStatusEnum.InTreatment.ToString()) // still restrict to admitted
        .AsQueryable();

    if (!string.IsNullOrEmpty(searchQuery))
    {
        var sq = searchQuery.ToLower();
        query = query.Where(p =>
            (p.Firstname ?? "").ToLower().Contains(sq) ||
            (p.Lastname ?? "").ToLower().Contains(sq) ||
            p.PatientId.ToString().Contains(sq));
    }

    if (!string.IsNullOrEmpty(status))
    {
        var inProgressVal = PsychiatricEnumStatus.InProgress.ToString();
        var completedVal  = PsychiatricEnumStatus.Completed.ToString();

        if (status == "Pending")
        {
            // Pending = Admitted (no started assessment yet or latest still null/Pending)
            query = query.Where(p =>
                p.PatientStatus == admitted &&
                (
                    !p.PsychiatricAssessments.Any() ||
                    p.PsychiatricAssessments
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefault().Status == null ||
                    p.PsychiatricAssessments
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefault().Status == "Pending"
                ));
        }
        else if (status.Equals("InProgress", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p =>
                p.PsychiatricAssessments.Any(a => a.Status == inProgressVal));
        }
        else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p =>
                p.PsychiatricAssessments.Any(a => a.Status == completedVal));
        }
        else
        {
            // fallback: direct match on assessment status
            query = query.Where(p =>
                p.PsychiatricAssessments.Any(a => a.Status == status));
        }
    }

    if (string.IsNullOrEmpty(sortBy) || sortBy == "Name")
    {
        query = (sortOrder == "ascending"
            ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
            : query.OrderByDescending(p => p.Firstname).ThenByDescending(p => p.Lastname));
    }
    else if (sortBy == "ScheduledDate")
    {
        query = (sortOrder == "ascending"
            ? query.OrderBy(p => p.CreatedAt)
            : query.OrderByDescending(p => p.CreatedAt));
    }

    int totalItems = await query.CountAsync();
    int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
    int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));

    var patientList = await query
        .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
        .Take(pageSize > 0 ? pageSize.Value : totalItems)
        .ToListAsync();

    var psychiatricViewModels = patientList
        .Select(p =>
        {
            var assessment = p.PsychiatricAssessments
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();
            return new PsychiatricAssessmentViewModel
            {
                PatientId = p.PatientId,
                FullName = $"{p.Firstname} {p.Lastname}",
                Type = assessment?.Type ?? "-",
                Date = assessment?.Date ?? p.CreatedAt,
                CompletedDate = assessment?.CompletedDate,
                Status = assessment?.Status ?? "Pending"
            };
        }).ToList();

    ViewBag.CurrentPage = currentPage;
    ViewBag.PageSize = pageSize ?? 10;
    ViewBag.SearchQuery = searchQuery;
    ViewBag.Status = status;
    ViewBag.SortOrder = sortOrder ?? "descending";
    ViewBag.SortBy = sortBy;
    ViewBag.TotalPages = totalPages;
    ViewBag.TotalPatientCount = totalItems;
    ViewBag.OnlyAdmitted = true;

    return View(psychiatricViewModels);
        }

        [HttpGet]
        public IActionResult SortBy(string sortBy, string sortOrder, string searchQuery, string status, int page = 1, int pageSize = 10)
        {
            return RedirectToAction("Index", new { sortBy, sortOrder, searchQuery, status, page, pageSize });
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
                           .Include(a => a.ProblemLists) // load problem list
                           .Include(a => a.DiagnosisLists) // load diagnosis list
                           .FirstOrDefaultAsync(a => a.PatientId == id);

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

                // Map to nested properties to match the view's asp-for bindings
                ChiefComplaint = assessment?.ChiefComplaint,
                HistoryOfPresentIllness = assessment?.HistoryOfPresentIllness,
                PersonalAndFamilyHistory = assessment?.PersonalAndFamilyHistory,
                MentalStatusExamination = assessment?.MentalStatusExamination,
                Impression = assessment?.Impression,
            };

            // Map problem list (as a list of strings)
            var problems = assessment?.ProblemLists?
                          .Select(pl => pl.Problem)
                          .Where(p => !string.IsNullOrWhiteSpace(p))
                          .ToList() ?? new List<string>();
            vm.ProblemList.Problems = problems;

            // Map diagnosis list (as a list of strings)
            var diagnoses = assessment?.DiagnosisLists?
                            .Select(dl => dl.Diagnosis)
                            .Where(d => !string.IsNullOrWhiteSpace(d))
                            .ToList() ?? new List<string>();
            vm.PsyDiagnosisList.Diagnosis = diagnoses;

            return View("PsychiatricAssessmentForm", vm);
        }

        //Saving Chief complaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavingChiefComplaint(PsychiatricAssessmentViewModel model)
        {
            int PatientId = model.PatientId;
            string ChiefComplaint = model.ChiefComplaint;
            var assessment = await _context.PsychiatricAssessments.FirstOrDefaultAsync(a => a.PatientId == PatientId);
            if (assessment != null)
            {
                assessment.ChiefComplaint = ChiefComplaint;
                assessment.Status = PsychiatricEnumStatus.InProgress.ToString();
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Chief Complaint saved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Assessment not found for the specified patient.";
                var newAssessment = new PsychiatricAssessment
                {
                    PatientId = PatientId,
                    ChiefComplaint = ChiefComplaint,
                    Status = PsychiatricEnumStatus.InProgress.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };
                _context.PsychiatricAssessments.Add(newAssessment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Chief Complaint saved successfully.";
            }

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(PatientId);
            await _activityService.LogAsync(user,
                "Saved Chief Complaint",
                $"Updated chief complaint for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Chief Complaint saved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("PsychiatricAssessmentForm", new { id = PatientId });
        }

        //Saving History of Present Illness
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavingHistoryOfPresent(PsychiatricAssessmentViewModel model)
        {
            int PatientId = model.PatientId;
            string HistoryOfPresentIllness = model.HistoryOfPresentIllness;
            var assessment = await _context.PsychiatricAssessments.FirstOrDefaultAsync(a => a.PatientId == PatientId);
            if (assessment != null)
            {
                assessment.HistoryOfPresentIllness = HistoryOfPresentIllness;
                assessment.Status = PsychiatricEnumStatus.InProgress.ToString();
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "History of Present Illness saved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Assessment not found for the specified patient.";
                var newAssessment = new PsychiatricAssessment
                {
                    PatientId = PatientId,
                    HistoryOfPresentIllness = HistoryOfPresentIllness,
                    Status = PsychiatricEnumStatus.InProgress.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };
                _context.PsychiatricAssessments.Add(newAssessment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "History of Present Illness saved successfully.";
            }

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(PatientId);
            await _activityService.LogAsync(user,
                "Saved History of Present Illness",
                $"Updated psychiatric HPI for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Psych HPI saved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

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
            else
            {
                var newAssessment = new PsychiatricAssessment
                {
                    PatientId = model.PatientId,
                    PersonalAndFamilyHistory = model.PersonalAndFamilyHistory,
                    Type = "Psychiatric Assessment",
                    Status = PsychiatricEnumStatus.InProgress.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };
                _context.PsychiatricAssessments.Add(newAssessment);
                await _context.SaveChangesAsync();
                assessment = newAssessment;
            }

            assessment.Status = PsychiatricEnumStatus.InProgress.ToString();
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(model.PatientId);
            await _activityService.LogAsync(user,
                "Saved Personal & Family History",
                $"Updated personal/family history for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Personal & Family history saved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            TempData["SuccessMessage"] = "Personal and Family History saved successfully.";
            return RedirectToAction("PsychiatricAssessmentForm", new { id = model.PatientId });
        }

        //Saving mental status
        [HttpPost]
        public async Task<IActionResult> SavingMentalStatus(PsychiatricAssessmentViewModel model)
        {
            var assessment = await _context.PsychiatricAssessments.FirstOrDefaultAsync(d => d.PatientId == model.PatientId);

            if (assessment == null)
            {
                assessment = new PsychiatricAssessment
                {
                    PatientId = model.PatientId,
                    MentalStatusExamination = model.MentalStatusExamination,
                    Status = PsychiatricEnumStatus.InProgress.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };
                _context.PsychiatricAssessments.Add(assessment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mental Status saved successfully.";
            }
            else
            {
                assessment.MentalStatusExamination = model.MentalStatusExamination;
                assessment.Status = PsychiatricEnumStatus.InProgress.ToString();
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mental Status saved successfully.";
            }

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(model.PatientId);
            await _activityService.LogAsync(user,
                "Saved Mental Status Examination",
                $"Updated psychiatric MSE for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Psych MSE saved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("PsychiatricAssessmentForm", new { id = model.PatientId });
        }

        //Saving Impression
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavingImpression(PsychiatricAssessmentViewModel model)
        {
            var assessment = await _context.PsychiatricAssessments.FirstOrDefaultAsync(a => a.PatientId == model.PatientId);
            if (assessment == null)
            {
                assessment = new PsychiatricAssessment
                {
                    PatientId = model.PatientId,
                    Impression = model.Impression,
                    Status = PsychiatricEnumStatus.InProgress.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };
                _context.PsychiatricAssessments.Add(assessment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Impression saved successfully.";
            }
            else
            {
                assessment.Impression = model.Impression;
                assessment.Status = PsychiatricEnumStatus.InProgress.ToString();
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Impression saved successfully.";
            }

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(model.PatientId);
            await _activityService.LogAsync(user,
                "Saved Impression",
                $"Updated impression for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Impression saved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("PsychiatricAssessmentForm", new { id = model.PatientId });
        }

        // POST: Save problem/diagnosis list for an assessment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProblemList(PsychiatricAssessmentViewModel model)
        {
            if (model == null)
                return BadRequest();

            var incomingProblems = model.ProblemList?.Problems ?? new List<string>();
            // normalize/trim and remove empty entries
            var cleaned = incomingProblems
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim())
                        .ToList();

            // find or create assessment record for the patient
            var assessment = await _context.PsychiatricAssessments
                                        .FirstOrDefaultAsync(a => a.PatientId == model.PatientId);

            if (assessment == null)
            {
                assessment = new PsychiatricAssessment
                {
                    PatientId = model.PatientId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PsychiatricAssessments.Add(assessment);
                await _context.SaveChangesAsync();
            }

            try
            {
                // use the concrete entity type defined in Models (PsyProblemList)
                var problemSet = _context.Set<PsyProblemList>();

                // Retain existing rows. Add only new problems (case-insensitive dedupe).
                var existingTexts = await problemSet
                    .Where(p => p.PsychiatricAssessmentId == assessment.PsychiatricAssessmentId)
                    .Select(p => p.Problem)
                    .ToListAsync();

                // Normalize incoming and remove duplicates inside the incoming payload
                var incomingUnique = cleaned
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Add only those not already present
                foreach (var p in incomingUnique)
                {
                    if (existingTexts.Any(e => string.Equals(e?.Trim(), p, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    var item = new PsyProblemList
                    {
                        PsychiatricAssessmentId = assessment.PsychiatricAssessmentId,
                        Problem = p,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                    problemSet.Add(item);
                }
 
                 //Update status to In Progress
                 assessment.Status = PsychiatricEnumStatus.InProgress.ToString();
 
                 await _context.SaveChangesAsync();
                 TempData["SuccessMessage"] = "Problem saved successfully.";
             }
             catch (Exception ex)
             {
                 // prefer logging if available; surface a friendly message
                 TempData["Error"] = "Error saving problem: " + ex.Message;
             }

            // --- log ---
            var user = User?.Identity?.Name ?? "System"; // <-- added
            var pat = await _context.Patients.FindAsync(model.PatientId); // <-- added
            await _activityService.LogAsync(user, // <-- added
                "Saved Psychiatric Problem List",
                $"Updated psychiatric problem list for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user, // <-- added
                $"Psych problem list saved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("PsychiatricAssessmentForm", new { id = model.PatientId });
        }

       [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDiagnosis(PsychiatricAssessmentViewModel model)
        {
            if (model == null)
                return BadRequest();

            var incomingDiagnoses = model.PsyDiagnosisList?.Diagnosis ?? new List<string>();
            // normalize/trim and remove empty entries
            var cleaned = incomingDiagnoses
                          .Where(p => !string.IsNullOrWhiteSpace(p))
                          .Select(p => p.Trim())
                          .ToList();

            // find or create assessment record for the patient
            var assessment = await _context.PsychiatricAssessments
                                           .FirstOrDefaultAsync(a => a.PatientId == model.PatientId);

            if (assessment == null)
            {
                assessment = new PsychiatricAssessment
                {
                    PatientId = model.PatientId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PsychiatricAssessments.Add(assessment);
                await _context.SaveChangesAsync();
            }

            try
            {
                // use the concrete entity type defined in Models (PsyDiagnosisList)
                var diagnosisSet = _context.Set<PsyDiagnosisList>();

                // remove existing entries for this assessment
                var existing = await diagnosisSet
                                     .Where(p => p.PsychiatricAssessmentId == assessment.PsychiatricAssessmentId)
                                     .ToListAsync();

                if (existing.Any())
                {
                    diagnosisSet.RemoveRange(existing);
                    await _context.SaveChangesAsync();
                }

                // The UI posts newest-first (index 0 = newest). Persist in that same order.
                foreach (var p in cleaned)
                {
                    var item = new PsyDiagnosisList
                    {
                        PsychiatricAssessmentId = assessment.PsychiatricAssessmentId,
                        Diagnosis = p
                    };
                    diagnosisSet.Add(item);
                }

                //Update status to In Progress
                assessment.Status = PsychiatricEnumStatus.InProgress.ToString();


                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Diagnosis saved successfully.";
            }
            catch (Exception ex)
            {
                // prefer logging if available; surface a friendly message
                TempData["Error"] = "Error saving diagnosis: " + ex.Message;
            }

            // --- log ---
            var user = User?.Identity?.Name ?? "System"; // <-- added
            var pat = await _context.Patients.FindAsync(model.PatientId); // <-- added
            await _activityService.LogAsync(user, // <-- added
                "Saved Psychiatric Diagnosis",
                $"Updated psychiatric diagnosis list for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user, // <-- added
                $"Psych diagnosis saved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("PsychiatricAssessmentForm", new { id = model.PatientId });
        }

      [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssessment(int patientId)
        {
            // Find the assessment by patientId
            var assessment = await _context.PsychiatricAssessments
                                        .FirstOrDefaultAsync(a => a.PatientId == patientId);

            if (assessment == null)
            {
                TempData["Error"] = "Assessment not found for the specified patient.";
                return RedirectToAction("Index", "PsychiatricAssessment");
            }

            // Update status and completed date
            assessment.Status = PsychiatricEnumStatus.Completed.ToString();
            assessment.CompletedDate = DateTime.UtcNow;

            // Update patient status to Intreatment
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (patient != null)
            {
                patient.PatientStatus = PatientStatusEnum.InTreatment.ToString();
                _context.Patients.Update(patient);
            }

            // Mark related NewAppointment as Completed
            var appointment = await _context.NewAppointments
                .FirstOrDefaultAsync(a => a.PatientId == patientId && a.Type == "Psychiatric Assessment" && a.Status != "Completed");
            if (appointment != null)
            {
                appointment.Status = SafehavenPMS.Enum.AppointmentEnum.Completed.ToString();
                _context.NewAppointments.Update(appointment);
            }

            // Save changes
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System"; // <-- added
            var pat = await _context.Patients.FindAsync(patientId); // <-- added
            await _activityService.LogAsync(user, // <-- added
                "Submitted Psychiatric Assessment",
                $"Submitted psychiatric assessment for {PatientFullName(pat)}",
                "PsychiatricAssessment",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user, // <-- added
                $"Psych assessment submitted for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("Index", "PsychiatricAssessment");
        }
    }   
}
