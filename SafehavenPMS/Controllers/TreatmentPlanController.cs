using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SafehavenPMS.Services; // <-- added

namespace SafehavenPMS.Controllers
{
[Authorize]
    public class TreatmentPlanController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly ActivityLogService _activityService; // <-- added

        private static string PatientFullName(Patient p) => p == null ? "" : $"{p.Firstname} {p.Lastname}"; // <-- added

        public TreatmentPlanController(SafehavenPMSContext context, ActivityLogService activityService) // <-- modified
        {
            _context = context;
            _activityService = activityService; // <-- added
        }

        public IActionResult Index()
        {
            return View();
        }

        
        // Add Psychiatric Problem
        [HttpPost]
        public async Task<IActionResult> AddProblem(int patientId, string problemText, int psychiatricAssessmentId)
        {
            if (string.IsNullOrWhiteSpace(problemText))
                return BadRequest("Problem description is required.");

            // Check if PsychiatricAssessmentId exists
            var assessmentExists = await _context.PsychiatricAssessments
                .AnyAsync(a => a.PsychiatricAssessmentId == psychiatricAssessmentId);

            if (!assessmentExists)
                return BadRequest("Invalid PsychiatricAssessmentId.");

            var problem = new PsyProblemList
            {
                PsychiatricAssessmentId = psychiatricAssessmentId,
                Problem = problemText,
                Status = "Active",
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
            };

            _context.PsyProblemLists.Add(problem);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(patientId);
            await _activityService.LogAsync(user,
                "Added Psychiatric Problem",
                $"Added problem '{problemText}' for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Problem added for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> EditProblem(int patientId, int psyProblemListId, string problemText)
        {
            var problem = await _context.PsyProblemLists.FirstOrDefaultAsync(p => p.PsyProblemListId == psyProblemListId);
            if (problem == null)
                return NotFound();

            problem.Problem = problemText;
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.PsyProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(patientId);
            await _activityService.LogAsync(user,
                "Edited Psychiatric Problem",
                $"Edited problem (ID {psyProblemListId}) for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Problem updated for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkProblemResolved(int patientId, int psyProblemListId)
        {
            var problem = await _context.PsyProblemLists.FirstOrDefaultAsync(p => p.PsyProblemListId == psyProblemListId);
            if (problem == null)
                return NotFound();

            problem.Status = "Resolved";
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.PsyProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(patientId);
            await _activityService.LogAsync(user,
                "Resolved Psychiatric Problem",
                $"Marked problem (ID {psyProblemListId}) as Resolved for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Problem resolved for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkProblemInactive(int patientId, int psyProblemListId)
        {
            var problem = await _context.PsyProblemLists.FirstOrDefaultAsync(p => p.PsyProblemListId == psyProblemListId);
            if (problem == null)
                return NotFound();

            problem.Status = "Inactive";
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.PsyProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(patientId);
            await _activityService.LogAsync(user,
                "Inactivated Psychiatric Problem",
                $"Marked problem (ID {psyProblemListId}) as Inactive for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Problem inactive for {PatientFullName(pat)}",
                type: "Info");
            // --- end ---

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkProblemActive(int patientId, int psyProblemListId)
        {
            var problem = await _context.PsyProblemLists.FirstOrDefaultAsync(p => p.PsyProblemListId == psyProblemListId);
            if (problem == null)
                return NotFound();

            problem.Status = "Active";
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.PsyProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(patientId);
            await _activityService.LogAsync(user,
                "Activated Psychiatric Problem",
                $"Marked problem (ID {psyProblemListId}) as Active for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Problem active for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> AddIntervention(
            int patientId,
            int problemId,
            DateTime StartDate,
            int ServiceModality,
            string DurationFrequency,
            string Description)
        {
            // Validate required fields
            if (ServiceModality == 0)
                return BadRequest("Service is required.");

            string status = StartDate > DateTime.Now ? "Not Started" : "Active";

            var intervention = new Intervention
            {
                PatientId = patientId,
                PsyProblemListId = problemId,
                ServiceId = ServiceModality,
                DurationFrequency = DurationFrequency,
                Description = Description,
                Status = status,
                StartDate = StartDate,
                NotedBy = User.Identity?.Name ?? "System",
                DateAdded = DateTime.Now
            };

            _context.Interventions.Add(intervention);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(patientId);
            await _activityService.LogAsync(user,
                "Added Intervention",
                $"Added intervention (Problem ID {problemId}, Service {ServiceModality}) for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Intervention added for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMedicationDiscontinued(int id)
        {
            var medication = await _context.MedicationOrders.FindAsync(id);
            if (medication == null)
            {
                TempData["ErrorMessage"] = "Medication order not found.";
                return RedirectToAction("Index", "PatientProfile");
            }

            medication.Status = "Discontinued";
            _context.Update(medication);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(medication.PatientId);
            await _activityService.LogAsync(user,
                "Discontinued Medication",
                $"Medication order {id} marked Discontinued for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Warning",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Medication discontinued for {PatientFullName(pat)}",
                type: "Info");
            // --- end ---

            TempData["SuccessMessage"] = "Medication order marked as discontinued.";
            return RedirectToAction("Index", "PatientProfile", new { id = medication.PatientId });
        }

        //Action to mark medication as completed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMedicationCompleted(int id)
        {
            var medication = await _context.MedicationOrders.FindAsync(id);
            if (medication == null)
            {
                TempData["ErrorMessage"] = "Medication order not found.";
                return RedirectToAction("Index", "PatientProfile");
            }

            medication.Status = "Completed";
            _context.Update(medication);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(medication.PatientId);
            await _activityService.LogAsync(user,
                "Completed Medication",
                $"Medication order {id} marked Completed for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Medication completed for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            TempData["SuccessMessage"] = "Medication order marked as completed.";
            return RedirectToAction("Index", "PatientProfile", new { id = medication.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicationOrder(int id)
        {
            var medication = await _context.MedicationOrders.FindAsync(id);
            if (medication == null)
            {
                TempData["ErrorMessage"] = "Medication order not found.";
                return RedirectToAction("Index", "PatientProfile");
            }

            medication.Status = "Removed";
            _context.Update(medication);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(medication.PatientId);
            await _activityService.LogAsync(user,
                "Removed Medication",
                $"Medication order {id} marked Removed for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Medication removed for {PatientFullName(pat)}",
                type: "Warning");
            // --- end ---

            TempData["SuccessMessage"] = "Medication order removed.";
            return RedirectToAction("Index", "PatientProfile", new { id = medication.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMedicationActive(int id)
        {
            var medication = await _context.MedicationOrders.FindAsync(id);
            if (medication == null)
            {
                TempData["ErrorMessage"] = "Medication order not found.";
                return RedirectToAction("Index", "PatientProfile");
            }

            medication.Status = "Active";
            _context.Update(medication);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(medication.PatientId);
            await _activityService.LogAsync(user,
                "Activated Medication",
                $"Medication order {id} marked Active for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Medication active for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            TempData["SuccessMessage"] = "Medication order marked as active.";
            return RedirectToAction("Index", "PatientProfile", new { id = medication.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInterventionDiscontinued(int id)
        {
            var intervention = await _context.Interventions.FindAsync(id);
            if (intervention == null)
            {
                TempData["ErrorMessage"] = "Intervention not found.";
                return RedirectToAction("Index", "PatientProfile");
            }

            intervention.Status = "Discontinued";
            _context.Update(intervention);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(intervention.PatientId);
            await _activityService.LogAsync(user,
                "Discontinued Intervention",
                $"Intervention {id} marked Discontinued for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Warning",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Intervention discontinued for {PatientFullName(pat)}",
                type: "Info");
            // --- end ---

            TempData["SuccessMessage"] = "Intervention marked as discontinued.";
            return RedirectToAction("Index", "PatientProfile", new { id = intervention.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInterventionCompleted(int id)
        {
            var intervention = await _context.Interventions.FindAsync(id);
            if (intervention == null)
            {
                TempData["ErrorMessage"] = "Intervention not found.";
                return RedirectToAction("Index", "PatientProfile");
            }

            intervention.Status = "Completed";
            _context.Update(intervention);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(intervention.PatientId);
            await _activityService.LogAsync(user,
                "Completed Intervention",
                $"Intervention {id} marked Completed for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Intervention completed for {PatientFullName(pat)}",
                type: "Success");
            // --- end ---

            TempData["SuccessMessage"] = "Intervention marked as completed.";
            return RedirectToAction("Index", "PatientProfile", new { id = intervention.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIntervention(int id)
        {
            var intervention = await _context.Interventions.FindAsync(id);
            if (intervention == null)
            {
                TempData["ErrorMessage"] = "Intervention not found.";
                return RedirectToAction("Index", "PatientProfile");
            }

            intervention.Status = "Removed";
            _context.Update(intervention);
            await _context.SaveChangesAsync();

            // --- log ---
            var user = User?.Identity?.Name ?? "System";
            var pat = await _context.Patients.FindAsync(intervention.PatientId);
            await _activityService.LogAsync(user,
                "Removed Intervention",
                $"Intervention {id} marked Removed for {PatientFullName(pat)}",
                "TreatmentPlan",
                "Info",
                pat?.PatientId);
            await _activityService.NotifyAsync(user,
                $"Intervention removed for {PatientFullName(pat)}",
                type: "Warning");
            // --- end ---

            TempData["SuccessMessage"] = "Intervention removed.";
            return RedirectToAction("Index", "PatientProfile", new { id = intervention.PatientId });
        }
    }
}

