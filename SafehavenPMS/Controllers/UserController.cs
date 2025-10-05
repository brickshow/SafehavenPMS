using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using SafehavenPMS.Data;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Controllers
{
[Authorize]
    public class UserController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(SafehavenPMSContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Index with search / filters / paging to match the Index.cshtml logic
        public async Task<IActionResult> Index(string searchQuery = "", string role = "", string showInactive = "", int page = 1, int pageSize = 10)
        {
            var q = _context.Users
                .Include(u => u.ClinicalStaff)
                .AsQueryable();

            // filter active by default
            var includeInactive = !string.IsNullOrEmpty(showInactive) && (showInactive == "1" || showInactive.Equals("true", StringComparison.OrdinalIgnoreCase));
            if (!includeInactive)
                q = q.Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(role))
                q = q.Where(u => u.Role == role);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var s = searchQuery.Trim();
                q = q.Where(u =>
                    u.Username.Contains(s) ||
                    (u.Email != null && u.Email.Contains(s)) ||
                    (u.ClinicalStaff != null && (u.ClinicalStaff.Firstname + " " + u.ClinicalStaff.Lastname).Contains(s))
                );
            }

            var total = await q.CountAsync();

            // handle "All" when pageSize == 0
            int take = pageSize <= 0 ? (int)total : pageSize;
            int currentPage = Math.Max(1, page);
            int totalPages = take == 0 ? 1 : (int)Math.Ceiling(total / (double)take);

            var items = await q.OrderBy(u => u.Username)
                               .Skip((currentPage - 1) * take)
                               .Take(take)
                               .ToListAsync();

            // populate ViewBag used by the view
            ViewBag.SearchQuery = searchQuery ?? string.Empty;
            ViewBag.Role = role ?? string.Empty;
            // always set a string value to avoid mixed-type comparisons in Razor
            ViewBag.ShowInactive = includeInactive ? "1" : "0";
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalUserCount = total;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }

        //Add user step1 
        public IActionResult NewUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id, string searchQuery = "", string role = "", string showInactive = "", int page = 1, int pageSize = 10)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", new { page, pageSize, searchQuery, role, showInactive });
            }

            if (!user.IsActive)
            {
                TempData["Error"] = "User is already inactive.";
                return RedirectToAction("Index", new { page, pageSize, searchQuery, role, showInactive });
            }

            user.IsActive = false;
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User deactivated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate user {UserId}", id);
                TempData["Error"] = "Failed to deactivate user.";
            }

            return RedirectToAction("Index", new { page, pageSize, searchQuery, role, showInactive });
        }
    }
}

