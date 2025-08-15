using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SafehavenPMS.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public AppointmentController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public IActionResult ScheduleAppointment()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDate(DateTime selectedDate)
        {
            var dayName = selectedDate.DayOfWeek.ToString();

            var timeSlots = await _context.Availabilities
                .Where(a =>
                    // Date within range
                    selectedDate.Date >= a.StartDate.Date &&
                    (a.NoEndDate || (a.EndDate.HasValue && selectedDate.Date <= a.EndDate.Value.Date))
                )
                // Look inside Days matching the day name & marked as available
                .SelectMany(a => a.Days
                    .Where(d => d.DayName == dayName && d.IsAvailable)
                    .SelectMany(d => d.TimeSlots)
                )
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();

            ViewBag.SelectedDate = selectedDate;
            ViewBag.AvailableTimes = timeSlots;

            return View("ScheduleAppointment");
        }
    }
}