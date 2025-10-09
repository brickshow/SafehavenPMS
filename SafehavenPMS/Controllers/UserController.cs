using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public UserController(
            SafehavenPMSContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /User
        public async Task<IActionResult> Index(string searchQuery = "", int page = 1, int pageSize = 10)
        {
            var q = _context.Users
                .Include(u => u.ClinicalStaff)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var sq = searchQuery.ToLower();
                q = q.Where(u =>
                    u.Username.ToLower().Contains(sq) ||
                    (u.Email != null && u.Email.ToLower().Contains(sq)) ||
                    (u.ClinicalStaff != null &&
                     ((u.ClinicalStaff.Firstname + " " + u.ClinicalStaff.Lastname).ToLower().Contains(sq))));
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

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower()))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email required to send credentials.");
                return View(model);
            }

            var plainPassword = GenerateSecurePassword();

            var user = new User
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Role = model.Role,
                IsActive = model.IsActive,
                CreatedBy = User?.Identity?.Name
            };

            // Hash password like in AccountController (instantiate PasswordHasher directly)
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, plainPassword);
            user.PasswordSalt = null; // keep null for backward compatibility

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendStaffCredentialsAsync(user.Email, user.Username, plainPassword, user.Username);
                TempData["SuccessMessage"] = "User created. Credentials emailed.";
            }
            catch
            {
                TempData["Error"] = "User created but sending email failed.";
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
        public async Task<IActionResult> Deactivate(int id, string searchQuery, int page = 1, int pageSize = 10)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index), new { searchQuery, page, pageSize });
            }
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = User?.Identity?.Name;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User deactivated.";
            return RedirectToAction(nameof(Index), new { searchQuery, page, pageSize });
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

