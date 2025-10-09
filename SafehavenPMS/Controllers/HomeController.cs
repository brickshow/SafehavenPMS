using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.ViewModel.Dashboard;


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

