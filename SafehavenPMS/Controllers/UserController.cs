using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System.Security.Cryptography;
using System.Text;

namespace SafehavenPMS.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            SafehavenPMSContext context,
            IEmailService emailService,
            ILogger<UserController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /User
        public async Task<IActionResult> Index(string searchQuery = "", string role = "", string showInactive = "", int page = 1, int pageSize = 10)
        {
            var q = _context.Users
                .Include(u => u.ClinicalStaff)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var sq = searchQuery.ToLower();
                q = q.Where(u =>
                    u.Username.ToLower().Contains(sq) ||
                    (u.Email != null && u.Email.ToLower().Contains(sq)) ||
                    (u.Fullname != null && u.Fullname.ToLower().Contains(sq)) ||
                    (u.ClinicalStaff != null &&
                     ((u.ClinicalStaff.Firstname + " " + u.ClinicalStaff.Lastname).ToLower().Contains(sq))));
            }

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(u => u.Role == role);
            }

            // Apply active/inactive filter
            if (!string.IsNullOrWhiteSpace(showInactive) && showInactive == "1")
            {
                // Show all users (active and inactive)
            }
            else
            {
                // Show only active users by default
                q = q.Where(u => u.IsActive);
            }

            var total = await q.CountAsync();
            if (pageSize > 0)
            {
                q = q
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
            }

            ViewBag.TotalUserCount = total;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Role = role;
            ViewBag.ShowInactive = showInactive;
            ViewBag.TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 1;

            var list = await q.ToListAsync();
            return View(list);
        }

        // GET: /User/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email required to send credentials.");
                return View(model);
            }

            // ?? Generate secure password automatically
            var plainPassword = GenerateSecurePassword();

            // ?? Get latest UserID and generate username (SF-0000001 format)
            int lastId = await _context.Users
                .OrderByDescending(u => u.UserId)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            string newUsername = $"SF-{(lastId + 1).ToString("D7")}";

            // ?? Create new user
            var user = new User
            {
                Username = newUsername,
                Fullname = model.Fullname,
                Number = model.Number,
                Email = model.Email,
                Role = model.Role,
                IsActive = model.IsActive,
                CreatedBy = User?.Identity?.Name,
                CreatedAt = DateTime.UtcNow
            };

            // ?? Hash password
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, plainPassword);
            user.PasswordSalt = null; // backward compatibility

            // ?? Save to DB
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ?? Send credentials via email
            try
            {
                await _emailService.SendStaffCredentialsAsync(
                    user.Email,
                    user.Username,
                    plainPassword,
                    user.Fullname
                );
                TempData["SuccessMessage"] = $"User {user.Username} created successfully. Credentials sent to {user.Email}.";
            }
            catch
            {
                TempData["Error"] = $"User {user.Username} created but sending email failed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.ClinicalStaff)
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // POST: /User/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id, string searchQuery = "", string role = "", string showInactive = "", int page = 1, int pageSize = 10)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index), new { searchQuery, role, showInactive, page, pageSize });
            }
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = User?.Identity?.Name;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User deactivated.";
            return RedirectToAction(nameof(Index), new { searchQuery, role, showInactive, page, pageSize });
        }

        // GET: /User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Check if current user is admin
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Access denied. Only administrators can edit user accounts.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users
                .Include(u => u.ClinicalStaff)
                .FirstOrDefaultAsync(u => u.UserId == id);
            
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserEditViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                Number = user.Number,
                Username = user.Username,
                Fullname = user.Fullname,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(viewModel);
        }

        // POST: /User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            // Check if current user is admin
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Access denied. Only administrators can edit user accounts.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                // Reload the user data for display
                var user = await _context.Users
                    .Include(u => u.ClinicalStaff)
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);
                
                if (user != null)
                {
                    model.Username = user.Username;
                    model.Fullname = user.Fullname;
                    model.Role = user.Role;
                    model.IsActive = user.IsActive;
                }
                
                return View(model);
            }

            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (userToUpdate == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update only email and number
            userToUpdate.Email = model.Email;
            userToUpdate.Number = model.Number;
            userToUpdate.UpdatedAt = DateTime.UtcNow;
            userToUpdate.UpdatedBy = currentUser.Username;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User account for {userToUpdate.Fullname} has been updated successfully.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating user account");
                TempData["Error"] = "Unable to update user account. Please try again.";
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to get current user
        private async Task<User?> GetCurrentUserAsync()
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);
        }

        // ADD helper below existing HashPassword
        private static string GenerateSecurePassword(int length = 12)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%^&*?";
            string all = upper + lower + digits + symbols;

            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var chars = new char[length];

            for (int i = 0; i < length; i++)
                chars[i] = all[bytes[i] % all.Length];

            // Ensure at least one from each group
            chars[0] = upper[bytes[0] % upper.Length];
            if (length > 1) chars[1] = lower[bytes[1] % lower.Length];
            if (length > 2) chars[2] = digits[bytes[2] % digits.Length];
            if (length > 3) chars[3] = symbols[bytes[3] % symbols.Length];

            return new string(chars.OrderBy(_ => Guid.NewGuid()).ToArray());
        }
    }
}

