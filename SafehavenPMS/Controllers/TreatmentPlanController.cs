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


namespace SafehavenPMS.Controllers
{
[Authorize]
    public class TreatmentPlanController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public TreatmentPlanController(SafehavenPMSContext context)
        {
            _context = context;
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

            string status = string.Empty;
            //Save status based on start date
            if (StartDate > DateTime.Now)
            {
                status = "Not Started";
            }
            if (StartDate <= DateTime.Now)
            {
                status = "Active";
            }

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

            // Redirect back to patient profile or treatment plan view
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

            TempData["SuccessMessage"] = "Intervention removed.";
            return RedirectToAction("Index", "PatientProfile", new { id = intervention.PatientId });
        }
    }
}

