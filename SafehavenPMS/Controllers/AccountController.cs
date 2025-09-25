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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(AccountViewModel vm)
    {
        Console.WriteLine($"CreateAccount POST called. Username={vm?.Username}, RecoveryEmail={vm?.RecoveryEmail}, Role={vm?.Role}, ClinicalStaffId={vm?.ClinicalStaffId}");

        // If no password was provided in the form, generate one and remove the ModelState error
        if (string.IsNullOrWhiteSpace(vm?.Password))
        {
            vm.Password = GenerateSecurePassword(8);
            // remove any ModelState entry for Password so ModelState.IsValid can be true
            ModelState.Remove(nameof(vm.Password));
            Console.WriteLine("No password supplied — generated a temporary password.");
        }

        Console.WriteLine("ModelState.IsValid: " + ModelState.IsValid);
        foreach (var entry in ModelState)
        {
            var errors = entry.Value.Errors;
            if (errors.Count == 0)
            {
                Console.WriteLine($"ModelState[{entry.Key}] = OK");
            }
            else
            {
                for (int i = 0; i < errors.Count; i++)
                {
                    var err = errors[i];
                    var msg = !string.IsNullOrEmpty(err.ErrorMessage) ? err.ErrorMessage : (err.Exception?.Message ?? "<unknown>");
                    Console.WriteLine($"ModelState[{entry.Key}].Errors[{i}] = {msg}");
                }
            }
        }

        if (!ModelState.IsValid)
            return View(vm);

        if (await _context.Users.AnyAsync(u => u.Username == vm.Username))
        {
            ModelState.AddModelError(nameof(vm.Username), "Username is already taken.");
            Console.WriteLine("Username already taken: " + vm.Username);
            return View(vm);
        }

        ClinicalStaff staff = null;
        if (vm.ClinicalStaffId != null)
        {
            staff = await _context.ClinicalStaffs.FindAsync(vm.ClinicalStaffId.Value);
            if (staff == null)
            {
                Console.WriteLine("ClinicalStaff not found with ID: " + vm.ClinicalStaffId);
                return NotFound();
            }
        }

        var user = new User
        {
            Username = vm.Username,
            Email = vm.RecoveryEmail ?? staff?.Email,
            Role = vm.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ClinicalStaffID = vm.ClinicalStaffId,
        };

        // hash the provided (or generated) password
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, vm.Password);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            Console.WriteLine($"User saved to database. Username={user.Username}, Email={user.Email}, Role={user.Role}, ClinicalStaffID={user.ClinicalStaffID}");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error saving account to database");
            Console.WriteLine("Error saving account to database: " + ex.Message);
            ModelState.AddModelError(string.Empty, "Unable to save account.");
            return View(vm);
        }

        var toEmail = user.Email;
        var staffName = $"{staff?.Firstname} {staff?.Lastname}".Trim();
        if (!string.IsNullOrWhiteSpace(toEmail))
        {
            try
            {
                Console.WriteLine($"Attempting to send credentials email to {toEmail} (username={user.Username}).");
                await _email.SendStaffCredentialsAsync(toEmail, user.Username, vm.Password, staffName);
                Console.WriteLine("Credentials email sent (not logging password).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send credentials email to {Email}", toEmail);
                Console.WriteLine($"Failed to send credentials email to {toEmail}: {ex.Message}");
                // continue
            }
        }

        return RedirectToAction("Index", "ClinicalStaff");
    }
        
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
        {
            chars[i] = all[bytes[i] % all.Length];
        }

        // ensure at least one char from each required set for complexity
        chars[0] = upper[bytes[0] % upper.Length];
        chars[1] = lower[bytes[1] % lower.Length];
        chars[2] = digits[bytes[2] % digits.Length];
        chars[3] = symbols[bytes[3] % symbols.Length];

        return new string(chars);
    }
}

