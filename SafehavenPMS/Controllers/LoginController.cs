using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;

namespace SafehavenPMS.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly ILogger<LoginController> _logger;
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();

        public LoginController(SafehavenPMSContext context, ILogger<LoginController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Login POST invalid modelstate");
                return View("Login", model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                _logger.LogInformation("Login failed: user not found for '{Username}'", model.Username);
                return View("Login", model);
            }

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, model.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                _logger.LogInformation("Login failed: invalid password for userId={UserId}", user.UserId);
                return View("Login", model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
            _logger.LogInformation("User signed in: userId={UserId}", user.UserId);

            // Always redirect to the homepage after login
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out");
            return RedirectToAction("Login");
        }
    }
}