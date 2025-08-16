using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public AppointmentController(SafehavenPMSContext context)
        {
            _context = context;
        }

        // GET: ScheduleAppointment page
        public async Task<IActionResult> ScheduleAppointment(int id)
        {
            // Find patient by ID
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id);

            // Find the first clinical staff linked to this patient
            var staff = await _context.ClinicalStaffPatients
                               .Where(p => p.PatientId == id)
                               .Select(st => st.ClinicalStaff).FirstOrDefaultAsync();

            // If no patient or staff found, return empty view
            if (patient == null || staff == null)
                return View();

            // Build view model with patient and staff info
            var vm = new AppointmentViewModel
            {
                PatientId = patient.PatientId,
                PatientName = $"{patient.Firstname} {patient.MiddleName} {patient.Lastname}",
                ClinicalStaffID = staff.ClinicalStaffID,
                ClinicalStaffName = $"{staff.Firstname} {staff.MiddleName} {staff.Lastname}"
            };

            // Prevent null reference when rendering available times
            ViewBag.AvailableTimes = new List<object>();

            // Return view with model
            return View(vm);
        }

        // POST: ScheduleAppointment (form submission)
        [HttpPost]
        public async Task<IActionResult> ScheduleAppointment(AppointmentViewModel model)
        {
            // Placeholder for timeslot logic (not implemented yet)

            // Validate model state
            if (!ModelState.IsValid)
            {
                // Print validation errors to console
                foreach (var entry in ModelState)
                {
                    var key = entry.Key; // Field name
                    var errors = entry.Value.Errors; // Errors for that field

                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                    }
                }

                // Set error message for the view
                ViewBag.Error = "Error scheduling an appointment!";

                // Return view (no model passed back here)
                return View();
            }

            // Create a new Appointment entity from the model
            var appointment = new Appointment
            {
                PatientId = model.PatientId,
                ClinicalStaffID = model.ClinicalStaffID,
                AvailabilityId = model.AvailabilityId,
                VisitType = model.VisitType,
                Description = model.Description,
                Status = Enum.AppointmentEnum.Pending.ToString(), // Default to "Pending"
                CreatedAt = DateTime.Now
            };

            try
            {
                // Save the appointment
                await _context.Appointments.AddAsync(appointment);
                await _context.SaveChangesAsync();

                // 🔹 Mark the selected timeslot as unavailable
                var availability = await _context.Availabilities
                    .Include(a => a.Days)
                        .ThenInclude(d => d.TimeSlots)
                    .FirstOrDefaultAsync(a => a.AvailabilityId == model.AvailabilityId);

                if (availability != null)
                {
                    // Find the exact timeslot booked
                    var timeSlot = availability.Days
                        .SelectMany(d => d.TimeSlots)
                        .FirstOrDefault(ts => ts.TimeSlotId == model.TimeSlotId);

                    if (timeSlot != null)
                    {
                        timeSlot.IsAvailable = false; // mark as booked
                        _context.Update(availability);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving appointment: " + ex.Message);
                TempData["Error"] = "Error Adding Appointment!";
                return View(model);
            }


            // Redirect after success
            TempData["ToastMessage"] = "Appointment scheduled successfully!";
            TempData["ToastType"] = "success";
            // Redirect to Patient Index after scheduling
            return RedirectToAction("Index", "Patient");
        }

        // POST: Submit date from calendar
        [HttpPost]
        public async Task<IActionResult> SubmitDate(DateTime selectedDate, int patientId)
        {
            // Reload patient info
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);

            // Reload staff info linked to patient
            var staff = await _context.ClinicalStaffPatients
                            .Where(cp => cp.PatientId == patientId)
                            .Select(cp => cp.ClinicalStaff)
                            .FirstOrDefaultAsync();

            // If no patient or staff, return empty model to the view
            if (patient == null || staff == null)
                return View("ScheduleAppointment", new AppointmentViewModel());

            // Rebuild view model with patient + staff details
            var vm = new AppointmentViewModel
            {
                PatientId = patient.PatientId,
                PatientName = $"{patient.Firstname} {patient.MiddleName} {patient.Lastname}",
                ClinicalStaffID = staff.ClinicalStaffID,
                ClinicalStaffName = $"{staff.Firstname} {staff.MiddleName} {staff.Lastname}",
                // SelectedDate = selectedDate (optional, currently commented out)
            };

            // Get the name of the day (e.g., Monday, Tuesday)
            var dayName = selectedDate.DayOfWeek.ToString();

            // Query database for available time slots for this date
            var timeSlots = await _context.Availabilities
                .Where(a => selectedDate.Date >= a.StartDate.Date &&
                            (a.NoEndDate || (a.EndDate.HasValue && selectedDate.Date <= a.EndDate.Value.Date)))
                .SelectMany(a => a.Days
                    .Where(d => d.DayName == dayName && d.IsAvailable) // Only available days
                    .SelectMany(d => d.TimeSlots
                        .Where(tm => tm.IsAvailable)
                        .Select(ts => new
                        {
                            a.AvailabilityId, // Parent availability ID
                            ts.TimeSlotId,   // Timeslot ID
                            ts.StartTime,    // Start time
                            ts.EndTime       // End time
                })))
                .OrderBy(ts => ts.StartTime) // Sort slots by time
                .ToListAsync();

            // Pass selected date and available times to the view
            ViewBag.SelectedDate = selectedDate;
            ViewBag.AvailableTimes = timeSlots;

            // Return ScheduleAppointment view with rebuilt model
            return View("ScheduleAppointment", vm);
        }
    }
}