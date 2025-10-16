using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.Services;


public class AccountController : Controller
{
    private readonly SafehavenPMSContext _context;
    private readonly IEmailService _email;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SafehavenPMSContext context, IEmailService email, ILogger<AccountController> logger)
    {
        _context = context;
        _email = email;
        _logger = logger;
        _context = context;
    }



    //Action for Confirmation
    public async Task<IActionResult> Confirmation(int? id)
    {
        var staff = await _context.ClinicalStaffs.FirstOrDefaultAsync(i => i.ClinicalStaffID == id);

        //Mapped into ViewModel
        var viewModel = new AccountViewModel
        {
            ClinicalStaffId = staff?.ClinicalStaffID,
            Firstname = staff.Firstname,
            MiddleName = staff.MiddleName,
            Email = staff.Email,
            Lastname = staff.Lastname,
            Position = staff.Position,
            PhoneNumber = staff.PhoneNumber
        };

        return View(viewModel);
    }

    //public async Task<IActionResult> CreateAccount(int? id)
    //{
    //    if (id == null) return NotFound();

    //    var staff = await _context.ClinicalStaffs.FirstOrDefaultAsync(i => i.ClinicalStaffID == id);
    //    if (staff == null) return NotFound();

    //    var viewModel = new AccountViewModel
    //    {
    //        ClinicalStaffId = staff.ClinicalStaffID,
    //        Firstname = staff.Firstname,
    //        MiddleName = staff.MiddleName,
    //        Lastname = staff.Lastname,
    //        Email = staff.Email,
    //        Position = staff.Position,
    //        PhoneNumber = staff.PhoneNumber
    //    };

    //    return View(viewModel);
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> CreateAccount(int? id)
    //{
    //    var vm = await _context.ClinicalStaffs.FirstOrDefaultAsync(i => i.ClinicalStaffID == id);

    //    // Check if username already exists
    //    if (await _context.Users.AnyAsync(u => u.Username == vm.ClinicalStaffRefId))
    //    {
    //        ModelState.AddModelError("", $"An account already exists for Clinical Staff ID {vm.ClinicalStaffRefId}.");
    //        Console.WriteLine("Account already exists for " + vm.ClinicalStaffRefId);
    //        return View(vm);
    //    }


    //    var user = new User
    //    {
    //        Username = vm.ClinicalStaffRefId,
    //        Email = vm.Email,
    //        Role = vm.Position,
    //        IsActive = true,
    //        CreatedAt = DateTime.UtcNow,
    //        ClinicalStaffID = vm.ClinicalStaffID,
    //    };

    //    // Hash password
    //    var hasher = new PasswordHasher<User>();
    //    user.PasswordHash = hasher.HashPassword(user, vm.Password);

    //    try
    //    {
    //        _context.Users.Add(user);
    //        await _context.SaveChangesAsync();
    //        Console.WriteLine($"User saved: Username={user.Username}, Email={user.Email}, Role={user.Role}");
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        _logger.LogError(ex, "Error saving account to database");
    //        Console.WriteLine("Error saving account: " + ex.Message);
    //        ModelState.AddModelError(string.Empty, "Unable to save account.");
    //        return View(vm);
    //    }

    //    // Send credentials email
    //    var toEmail = user.Email;
    //    var staffName = $"{staff.Firstname} {staff.Lastname}".Trim();
    //    if (!string.IsNullOrWhiteSpace(toEmail))
    //    {
    //        try
    //        {
    //            Console.WriteLine($"Sending credentials to {toEmail}...");
    //            await _email.SendStaffCredentialsAsync(toEmail, user.Username, vm.Password, staffName);
    //            Console.WriteLine("Credentials email sent.");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Failed to send credentials email to {Email}", toEmail);
    //            Console.WriteLine($"Failed to send email: {ex.Message}");
    //        }
    //    }

    //    TempData["ToastMessage"] = $"Account created successfully for {staffName} ({username}).";
    //    return RedirectToAction("Index", "Cl


    //private static string GenerateSecurePassword(int length = 8)
    //{
    //    const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    //    const string lower = "abcdefghijkmnopqrstuvwxyz";
    //    const string digits = "23456789";
    //    const string symbols = "!@#$%*?-.";
    //    string all = upper + lower + digits + symbols;

    //    var bytes = new byte[length];
    //    using var rng = RandomNumberGenerator.Create();


    //    rng.GetBytes(bytes);

    //    var chars = new char[length];
    //    for (int i = 0; i < length; i++)
    //    {
    //        chars[i] = all[bytes[i] % all.Length];
    //    }

    //    // ensure at least one char from each required set for complexity
    //    chars[0] = upper[bytes[0] % upper.Length];
    //    chars[1] = lower[bytes[1] % lower.Length];
    //    chars[2] = digits[bytes[2] % digits.Length];
    //    chars[3] = symbols[bytes[3] % symbols.Length];

    //    return new string(chars);
    //}

    // Create Account page (GET)
    public async Task<IActionResult> CreateAccount(int? id)
    {
        if (id == null) return NotFound();

        var staff = await _context.ClinicalStaffs.FirstOrDefaultAsync(i => i.ClinicalStaffID == id);
        if (staff == null) return NotFound();

        var viewModel = new AccountViewModel
        {
            ClinicalStaffId = staff.ClinicalStaffID,
            Firstname = staff.Firstname,
            MiddleName = staff.MiddleName,
            Lastname = staff.Lastname,
            Email = staff.Email,
            Position = staff.Position,
            PhoneNumber = staff.PhoneNumber
        };

        return View(viewModel);
    }

    // Create Account (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccountConfirmed(int? id)
    {
        if (id == null) return NotFound();

        var staff = await _context.ClinicalStaffs.FirstOrDefaultAsync(i => i.ClinicalStaffID == id);
        if (staff == null) return NotFound();

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == staff.ClinicalStaffRefId))
        {
            ModelState.AddModelError("", $"An account already exists for Clinical Staff ID {staff.ClinicalStaffRefId}.");
            Console.WriteLine("Account already exists for " + staff.ClinicalStaffRefId);
            return View("CreateAccount", staff);
        }

        // Generate a secure password
        string generatedPassword = GenerateSecurePassword(8);
        string Fullname = staff.Firstname + " " + staff.Lastname;
        var user = new User
        {
            Username = staff.ClinicalStaffRefId,
            Email = staff.Email,
            Fullname = Fullname,
            Number = staff.PhoneNumber,
            Role = staff.Position,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ClinicalStaffID = staff.ClinicalStaffID
        };

        // Hash password
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, generatedPassword);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            Console.WriteLine($"User saved: Username={user.Username}, Email={user.Email}, Role={user.Role}");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error saving account to database");
            Console.WriteLine("Error saving account: " + ex.Message);
            ModelState.AddModelError(string.Empty, "Unable to save account.");
            return View("CreateAccount", staff);
        }

        // Send credentials via email
        var toEmail = user.Email;
        var staffName = $"{staff.Firstname} {staff.Lastname}".Trim();

        if (!string.IsNullOrWhiteSpace(toEmail))
        {
            try
            {
                Console.WriteLine($"Sending credentials to {toEmail}...");
                await _email.SendStaffCredentialsAsync(toEmail, user.Username, generatedPassword, staffName);
                Console.WriteLine("Credentials email sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send credentials email to {Email}", toEmail);
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        TempData["ToastMessage"] = $"Account created successfully for {staffName} ({user.Username}).";
        return RedirectToAction("Index", "ClinicalStaff");
    }

    // Helper for password generation
    private static string GenerateSecurePassword(int length = 8)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%*?-.";

        string all = upper + lower + digits + symbols;
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = all[bytes[i] % all.Length];

        // Ensure complexity
        chars[0] = upper[bytes[0] % upper.Length];
        chars[1] = lower[bytes[1] % lower.Length];
        chars[2] = digits[bytes[2] % digits.Length];
        chars[3] = symbols[bytes[3] % symbols.Length];

        return new string(chars);
    }
}


