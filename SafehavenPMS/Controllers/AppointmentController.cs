using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
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

        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                                      .Include(d => d.Day)
                                            .ThenInclude(t => t.TimeSlots)
                                       .Where(s => s.Status == "Pending")
                                       .ToListAsync();

            var model = new AppointmentPageViewModel
            {
                Appointments = appointments,

                // Map each Appointment -> AppointmentPendingApprovalViewModel
                PendingAppointments = appointments.Select(a => new AppointmentPendingApprovalViewModel
                {
                    AppointmentId = a.AppointmentId,
                    VisitType = a.VisitType,
                    StartTime = a.TimeSlot.StartTime,
                    EndTime = a.TimeSlot.EndTime,
                    DayName = a.Day.DayName,
                }).ToList()
            };

            return View(model);
        }

        //Action to get pending appointments

        // Action to get confirmed appointments for calendar
        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            // 1. Load appointments from DB, include Availability -> Days -> TimeSlots
            var appointments = await _context.Appointments
                .Include(a => a.Availability)
                    .ThenInclude(av => av.Days)
                        .ThenInclude(d => d.TimeSlots)
                // Only return confirmed appointments
                .Where(s => s.Status == Enum.AppointmentEnum.Confirmed.ToString())
                .ToListAsync();

            // 2. Transform appointments into calendar event objects
            var calendarData = appointments.Select(a =>
            {
                // Find the booked timeslot for this appointment
                var bookedSlot = a.Availability.Days
                    .SelectMany(d => d.TimeSlots)
                    .FirstOrDefault(ts => ts.TimeSlotId == a.TimeSlotId);

                // Find the day that contains this booked timeslot
                var day = a.Availability.Days
                    .FirstOrDefault(d => d.TimeSlots.Any(ts => ts.TimeSlotId == a.TimeSlotId));

                // If no matching slot or day found, skip this appointment
                if (bookedSlot == null || day == null)
                    return null;

                // Convert day name string (e.g., "Monday") into DayOfWeek enum
                if (!System.Enum.TryParse<DayOfWeek>(day.DayName, out var dayOfWeek))
                    return null;

                // Calculate the actual datetime of the appointment
                var startDateTime = NextDayOfWeek(dayOfWeek).Date + bookedSlot.StartTime;
                var endDateTime = NextDayOfWeek(dayOfWeek).Date + bookedSlot.EndTime;

                // Map appointment into calendar event object
                return new
                {
                    id = a.AppointmentId,  // Unique identifier
                    title = a.VisitType,   // Main label shown on calendar
                    start = startDateTime.ToString("yyyy-MM-ddTHH:mm:ss"), // ISO start
                    end = endDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),     // ISO end
                    color = a.Status == Enum.AppointmentEnum.Confirmed.ToString()
                        ? "#CBE5DC"  // greenish for confirmed
                        : "#FFD400", // yellow for others (e.g., pending)
                    extendedProps = new
                    {
                        visitType = a.VisitType, // Extra details for tooltip/popup
                        time = $"{bookedSlot.StartTime:hh\\:mm} - {bookedSlot.EndTime:hh\\:mm}"
                    }
                };
            })
            // Filter out null results (invalid data skipped above)
            .Where(x => x != null)
            .ToList();

            // 3. Return JSON for calendar frontend (e.g., FullCalendar)
            return Json(calendarData);
        }


        // Action to Confirm Appointment
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            // Find the appointment by ID, also load related Day and TimeSlot
            var appointment = await _context.Appointments
                .Include(a => a.Day)        // Eager load Day (to get DayName)
                .Include(a => a.TimeSlot)   // Eager load TimeSlot (to get Start/End time)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            // If no appointment found, return 404
            if (appointment == null)
            {
                return NotFound();
            }

            try
            {
                // Update the status to "Confirmed"
                appointment.Status = Enum.AppointmentEnum.Confirmed.ToString();

                // Update the record in the database
                _context.Update(appointment);

                // Save changes to DB
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error (you can replace Console.WriteLine with proper logging)
                Console.WriteLine("Error: " + ex);
            }

            // Get all appointments again after the update
            var appointments = await _context.Appointments
                .Include(a => a.Day)        // Include Day for each appointment
                .Include(a => a.TimeSlot)   // Include TimeSlot for each appointment
                .ToListAsync();

            // Build the ViewModel for Index
            var model = new AppointmentPageViewModel
            {
                // All appointments (used for calendar, etc.)
                Appointments = appointments,

                // Only "Pending" appointments for notification panel
                PendingAppointments = appointments
                    .Where(a => a.Status == Enum.AppointmentEnum.Pending.ToString()) // Filter only pending
                    .Select(a => new AppointmentPendingApprovalViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        VisitType = a.VisitType,
                        StartTime = a.TimeSlot.StartTime,
                        EndTime = a.TimeSlot.EndTime,
                        DayName = a.Day.DayName
                    })
                    .ToList()
            };

            // Return the Index view with updated model
            return View("Index", model);
        }

        // Action to Cancel Appointment
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            // Find the appointment by ID, include related Day and TimeSlot
            var appointment = await _context.Appointments
                .Include(a => a.Day)        // Include Day (for display purposes)
                .Include(a => a.TimeSlot)   // Include TimeSlot (for start/end times)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            // If no appointment found, return 404
            if (appointment == null)
            {
                return NotFound();
            }

            try
            {
                // Update the status to "Cancelled"
                appointment.Status = Enum.AppointmentEnum.Cancelled.ToString();

                // Update record in the database
                _context.Update(appointment);

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error (replace with proper logging later)
                Console.WriteLine("Error: " + ex);
            }

            // Reload all appointments after cancellation
            var appointments = await _context.Appointments
                .Include(a => a.Day)
                .Include(a => a.TimeSlot)
                .ToListAsync();

            // Build the updated ViewModel
            var model = new AppointmentPageViewModel
            {
                Appointments = appointments,
                PendingAppointments = appointments
                    .Where(a => a.Status == Enum.AppointmentEnum.Pending.ToString()) // still only pending
                    .Select(a => new AppointmentPendingApprovalViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        VisitType = a.VisitType,
                        StartTime = a.TimeSlot.StartTime,
                        EndTime = a.TimeSlot.EndTime,
                        DayName = a.Day.DayName
                    })
                    .ToList()
            };

            // Return the Index view with refreshed data
            return View("Index", model);
        }


        // Helper function
        public static DateTime NextDayOfWeek(DayOfWeek day)
        {
            var today = DateTime.Today;
            int daysUntil = ((int)day - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(daysUntil);
        }

        //
        public async Task<IActionResult> PendingAppointments()
        {
            var pending = await _context.Appointments
                .Where(a => a.Status == "Pending")
                .Select(a => new AppointmentPendingApprovalViewModel
                {
                    AppointmentId = a.AppointmentId,
                    VisitType = a.VisitType,
                    StartTime = a.TimeSlot.StartTime,
                    EndTime = a.TimeSlot.EndTime,
                })
                .ToListAsync();

            return PartialView("_Calendar", pending);
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

        [HttpPost]
        public async Task<IActionResult> ScheduleAppointment(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Log errors
                foreach (var entry in ModelState)
                    foreach (var error in entry.Value.Errors)
                        Console.WriteLine($"Field: {entry.Key} - Error: {error.ErrorMessage}");

                ViewBag.Error = "Error scheduling an appointment!";
                return View(model);
            }

            // Get the selected Availability with its Days and TimeSlots
            var availability = await _context.Availabilities
                .Include(a => a.Days)
                    .ThenInclude(d => d.TimeSlots)
                .FirstOrDefaultAsync(a => a.AvailabilityId == model.AvailabilityId);

            if (availability == null)
            {
                TempData["Error"] = "Selected availability not found!";
                return View(model);
            }

            // Find the Day that contains the selected TimeSlot
            var selectedDay = availability.Days
                .FirstOrDefault(d => d.TimeSlots.Any(ts => ts.TimeSlotId == model.TimeSlotId));

            if (selectedDay == null)
            {
                TempData["Error"] = "Selected time slot not found!";
                return View(model);
            }

            // Get the exact TimeSlot
            var selectedTimeSlot = selectedDay.TimeSlots
                .FirstOrDefault(ts => ts.TimeSlotId == model.TimeSlotId);

            if (selectedTimeSlot == null)
            {
                TempData["Error"] = "Time slot not found!";
                return View(model);
            }

            // Create the Appointment
            var appointment = new Appointment
            {
                PatientId = model.PatientId,
                ClinicalStaffID = model.ClinicalStaffID,
                AvailabilityId = availability.AvailabilityId,
                AvailabilityDayId = selectedDay.DayId,   // <-- store the Day FK
                TimeSlotId = selectedTimeSlot.TimeSlotId, // <-- store the TimeSlot FK
                VisitType = model.VisitType,
                Description = model.Description,
                Status = Enum.AppointmentEnum.Pending.ToString(),
                CreatedAt = DateTime.Now
            };

            // Save Appointment
            await _context.Appointments.AddAsync(appointment);

            // Mark the TimeSlot as unavailable
            selectedTimeSlot.IsAvailable = false;
            _context.Update(availability);

            // Change patient status
            var patient = await _context.Patients.FindAsync(model.PatientId);
            if (patient != null)
            {
                patient.PatientStatus = Enum.PatientStatusEnum.PendingAssesment.ToString();
                _context.Update(patient);
            }

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Appointment scheduled successfully!";
            TempData["ToastType"] = "success";
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