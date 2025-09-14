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
        
        [HttpPost]
        public async Task<IActionResult> AddGoal(int patientId, int problemListId, string description, DateTime? targetDate)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest("Goal description is required.");
            }

            var problem = await _context.ProblemLists
                .Include(p => p.Goals)
                .FirstOrDefaultAsync(p => p.ProblemListId == problemListId);

            if (problem == null)
            {
                return NotFound("Problem not found.");
            }

            var goal = new Goal
            {
                Description = description,
                TargetDate = targetDate,
                Status = "In Progress",
                NotedBy = User.Identity?.Name ?? "System",
                ProblemListId = problemListId,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
                // Add other fields if needed
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            // Optionally, return the updated goals list or a success message
            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> EditGoal(int patientId, int goalId, int problemListId, string description, DateTime? targetDate)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest("Goal description is required.");
            }

            var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.ProblemListId == problemListId);
            if (goal == null)
            {
                return NotFound("Goal not found.");
            }

            goal.Description = description;
            goal.TargetDate = targetDate;

            //TODO: Add audit fields for activity tracking
            goal.UpdatedAt = DateTime.Now;
            goal.UpdatedBy = User.Identity?.Name ?? "System";

            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkGoalAsCompleted(int patientId, int goalId, int problemListId)
        {
            var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.ProblemListId == problemListId);
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
        public async Task<IActionResult> DiscontinueGoal(int patientId, int goalId, int problemListId)
        {
            var goal = await _context.Goals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.ProblemListId == problemListId);
            if (goal == null)
                return NotFound();

            goal.Status = "Discontinue";
            goal.UpdatedAt = DateTime.Now;
            goal.UpdatedBy = User.Identity?.Name ?? "System";
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> AddProblem(int patientId, string Problems, int initialAssessmentFormId)
        {
            if (string.IsNullOrWhiteSpace(Problems))
                return BadRequest("Problem description is required.");

            var problem = new ProblemList
            {
                Problem = Problems,
                Status = "Active",
                InitialAssessmentFormId = initialAssessmentFormId, // <-- Set this!
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedAt = DateTime.Now
            };

            _context.ProblemLists.Add(problem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> EditProblem(int patientId, int ProblemListId, string Problems)
        {
            var problem = await _context.ProblemLists.FirstOrDefaultAsync(p => p.ProblemListId == ProblemListId);
            if (problem == null)
                return NotFound();

            problem.Problem = Problems;
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.ProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkProblemResolved(int patientId, int ProblemListId)
        {
            var problem = await _context.ProblemLists.FirstOrDefaultAsync(p => p.ProblemListId == ProblemListId);
            if (problem == null)
                return NotFound();

            problem.Status = "Resolved";
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.ProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkProblemInactive(int patientId, int ProblemListId)
        {
            var problem = await _context.ProblemLists.FirstOrDefaultAsync(p => p.ProblemListId == ProblemListId);
            if (problem == null)
                return NotFound();

            problem.Status = "Inactive";
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.ProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> MarkProblemActive(int patientId, int ProblemListId)
        {
            var problem = await _context.ProblemLists.FirstOrDefaultAsync(p => p.ProblemListId == ProblemListId);
            if (problem == null)
                return NotFound();

            problem.Status = "Active";
            problem.UpdatedAt = DateTime.Now;
            problem.UpdatedBy = User.Identity?.Name ?? "System";

            _context.ProblemLists.Update(problem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PatientProfile", new { id = patientId });
        }
    }
}
