using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;

namespace SafehavenPMS.Controllers
{
    [AllowAnonymous]
[Authorize]
    public class LoginController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly ILogger<LoginController> _logger;
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();
        private readonly IEmailService _emailService;

        public LoginController(SafehavenPMSContext context, ILogger<LoginController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
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
                new Claim(ClaimTypes.Name, user.Username ?? user.Email ?? "user")
            };

            // ensure role claim is added
            if (!string.IsNullOrWhiteSpace(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Trim()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var recEmail = await _context.Users
                .Where(u => u.Email == model.Email)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            var username = await _context.Users
                .Where(u => u.Email == model.Email)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();
            
            if (recEmail == null)
            {
                ModelState.AddModelError(string.Empty, "No account found with that email.");
                TempData["Error"] = "No account found with that email.";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Generate a random 6-digit code
                var otp = new Random().Next(100000, 999999).ToString();

                // Store OTP and email as needed (TempData, Session, DB, etc.)
                TempData["EmailForOtp"] = model.Email;
                TempData["OtpCode"] = otp;

                // Send OTP to email
                await _emailService.SendOtpAsync(username, model.Email, otp);

                // Redirect directly to EnterOtp page (no protocol/port logic)
                return RedirectToAction("EnterOtp", "Login");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult EnterOtp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EnterOtp(OtpViewModel model)
        {
            // Retrieve the OTP and email stored in TempData (or Session/DB as needed)
            var storedOtp = TempData["OtpCode"] as string;
            var email = TempData["EmailForOtp"] as string;

            // Keep TempData for next request (so user can retry if needed)
            TempData.Keep("OtpCode");
            TempData.Keep("EmailForOtp");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(email))
            {
                TempData["OtpError"] = "Session expired. Please request a new OTP.";
                return RedirectToAction("ForgotPassword");
            }

            if (model.Otp == storedOtp)
            {
                // OTP matched, allow user to reset password
                TempData.Remove("OtpCode"); // Optionally clear OTP
                TempData["ResetEmail"] = email; // Pass email to reset password page
                return RedirectToAction("ResetPassword");
            }
            else
            {
                TempData["OtpError"] = "Invalid OTP code. Please try again.";
                return RedirectToAction("EnterOtp");
            }
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            // Optionally check if ResetEmail is present in TempData
            if (TempData["ResetEmail"] == null)
            {
                return RedirectToAction("Login");
            }
            TempData.Keep("ResetEmail");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            // Retrieve email from TempData
            var email = TempData["ResetEmail"] as string;

            // Keep TempData for further requests if needed
            TempData.Keep("ResetEmail");

            if (string.IsNullOrEmpty(email))
            {
            TempData["ResetError"] = "Session expired. Please try again.";
            Console.WriteLine("ResetPassword error: Session expired, ResetEmail not found in TempData.");
            return RedirectToAction("ForgotPassword");
            }

            if (!ModelState.IsValid)
            {
            // Build a readable string of model state errors for console output
            var errors = new List<string>();
            foreach (var entry in ModelState)
            {
                foreach (var err in entry.Value.Errors)
                {
                var errMsg = err.ErrorMessage;
                if (string.IsNullOrEmpty(errMsg) && err.Exception != null)
                {
                    errMsg = err.Exception.Message;
                }
                errors.Add($"{entry.Key}: {errMsg}");
                }
            }

            Console.WriteLine("ResetPassword ModelState invalid: " + (errors.Count > 0 ? string.Join(" | ", errors) : "No specific errors found."));
            TempData["ResetError"] = "Please correct the errors and try again.";
            return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
            Console.WriteLine("ResetPassword error: Passwords do not match for email=" + email);
            TempData["ResetError"] = "Passwords do not match.";
            return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
            Console.WriteLine("ResetPassword error: User not found for email=" + email);
            TempData["ResetError"] = "User not found.";
            return RedirectToAction("ForgotPassword");
            }

            try
            {
            // Hash and update the new password
            user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            Console.WriteLine("ResetPassword success: Password updated for userId=" + user.UserId + " email=" + email);
            TempData["ResetSuccess"] = "Password reset successful! You can now log in.";
            return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
            Console.WriteLine("ResetPassword exception for email=" + email + ": " + ex);
            TempData["ResetError"] = "An error occurred while resetting the password. Please try again.";
            return View(model);
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            // simple view that informs user they lack permission
            return View("AccessDenied");
        }
    }
}
