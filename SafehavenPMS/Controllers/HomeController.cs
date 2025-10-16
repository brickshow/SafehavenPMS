using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.ViewModel.Dashboard;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using SafehavenPMS.Enum;

namespace SafehavenPMS.Controllers
{
[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SafehavenPMS.Data.SafehavenPMSContext _context;

        public HomeController(ILogger<HomeController> logger, SafehavenPMS.Data.SafehavenPMSContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var vm = new DashboardStatsViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(),
                NewPatientsThisMonth = await _context.Patients.CountAsync(p => p.CreatedAt.Month == now.Month && p.CreatedAt.Year == now.Year),
                Doctors = await _context.ClinicalStaffs.CountAsync(s => s.Position == "Physician" || s.Position == "Psychiatrist"),
                Nurses = await _context.ClinicalStaffs.CountAsync(s => s.Position == "Social Worker"),
                Coaches = await _context.ClinicalStaffs.CountAsync(s => s.Position == "Recovery Coach"),
                Appointments = await _context.NewAppointments.CountAsync(),
                Invoices = await _context.Invoices.CountAsync(),
                Users = await _context.Users.CountAsync()
            };

            // --- NEW: compute patient status counts and expose JSON to the view ---
            var grouped = await _context.Patients
                .GroupBy(p => p.PatientStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var enumNames = System.Enum.GetNames(typeof(PatientStatusEnum)).ToList();

            var labels = new List<string>();
            var values = new List<int>();
            foreach (var name in enumNames)
            {
                labels.Add(name);
                var entry = grouped.FirstOrDefault(g => string.Equals(g.Status ?? "", name, StringComparison.OrdinalIgnoreCase));
                values.Add(entry?.Count ?? 0);
            }

            // also include any statuses present in DB but not in enum
            var others = grouped
                .Where(g => !enumNames.Any(e => string.Equals(e, g.Status ?? "", StringComparison.OrdinalIgnoreCase)))
                .ToList();
            foreach (var o in others)
            {
                labels.Add(o.Status ?? "Unknown");
                values.Add(o.Count);
            }

            ViewBag.PatientStatusLabelsJson = JsonSerializer.Serialize(labels);
            ViewBag.PatientStatusValuesJson = JsonSerializer.Serialize(values);
            // -------------------------------------------------------------------

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

