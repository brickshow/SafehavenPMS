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

namespace SafehavenPMS.Controllers
{
    public class TreatmentPlanController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public TreatmentPlanController(SafehavenPMSContext context)
        {
            _context = context;
        }

        // Add Goal for PsychiatricAssessment Problem
        [HttpPost]
        public async Task<IActionResult> AddGoal(int patientId, int psyProblemListId, string description, DateTime? targetDate)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest("Goal description is required.");
            }

            var problem = await _context.PsyProblemLists
                .Include(p => p.Goals)
                .FirstOrDefaultAsync(p => p.PsyProblemListId == psyProblemListId);

            if (problem == null)
            {
                return NotFound("Psychiatric Problem not found.");
            }

            var goal = new Goal
            {
                Description = description,
                TargetDate = targetDate,
                Status = "In Progress",
                NotedBy = User.Identity?.Name ?? "System",
                PsyProblemListId = psyProblemListId,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> EditGoal(int patientId, int goalId, int psyProblemListId, string description, DateTime? targetDate)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest("Goal description is required.");
            }

            var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.PsyProblemListId == psyProblemListId);
            if (goal == null)
            {
                return NotFound("Goal not found.");
            }

            goal.Description = description;
            goal.TargetDate = targetDate;
            goal.UpdatedAt = DateTime.Now;
            goal.UpdatedBy = User.Identity?.Name ?? "System";

            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkGoalAsCompleted(int patientId, int goalId, int psyProblemListId)
        {
            var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.PsyProblemListId == psyProblemListId);
            if (goal == null)
                return NotFound();

            goal.Status = "Completed";
            goal.UpdatedAt = DateTime.Now;
            goal.UpdatedBy = User.Identity?.Name ?? "System";
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> DiscontinueGoal(int patientId, int goalId, int psyProblemListId)
        {
            var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.PsyProblemListId == psyProblemListId);
            if (goal == null)
                return NotFound();

            goal.Status = "Discontinue";
            goal.UpdatedAt = DateTime.Now;
            goal.UpdatedBy = User.Identity?.Name ?? "System";
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
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
            int ServiceType,
            int ServiceModality,
            string DurationFrequency,
            string Description)
        {
            // Validate required fields
            if (ServiceType == 0 || ServiceModality == 0)
                return BadRequest("Service Type and Service Modality are required.");

            var intervention = new Intervention
            {
                PatientId = patientId,
                PsyProblemListId = problemId,
                ServiceTypeId = ServiceType,
                ServiceId = ServiceModality,
                DurationFrequency = DurationFrequency,
                Description = Description,
                Status = "Active",
                NotedBy = User.Identity?.Name ?? "System",
                DateAdded = DateTime.Now
            };

            _context.Interventions.Add(intervention);
            await _context.SaveChangesAsync();

            // Redirect back to patient profile or treatment plan view
            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }
    }
}
