using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            // Fetch all pending appointments from the database
            // Include related entities: Staff, Patient, and Availability
            var appointments = await _context.Appointments
                .Include(a => a.Staff)          // Include doctor/clinical staff info
                .Include(a => a.Patient)        // Include patient info
                .Include(a => a.Availability)   // Include availability info (date/time slot)
                .Where(a => a.Status == "Pending") // Only pending appointments
                .ToListAsync();

            // Prepare the view model
            var model = new AppointmentPageViewModel
            {
                Appointments = appointments, // Store all appointments (optional for other uses)

                // Map appointments to the simplified PendingAppointments list for display
                PendingAppointments = appointments.Select(a => new AppointmentPendingApprovalViewModel
                {
                    AppointmentId = a.AppointmentId, // Appointment unique ID
                    PatientName = $"{a.Patient.Firstname} {a.Patient.Lastname}", // Full patient name
                    DoctorName = $"{a.Staff.Firstname} {a.Staff.Lastname}",       // Full doctor/staff name
                    AppointmentDate = a.AppointmentDate,
                    VisitType = a.VisitType,                                     // Type of visit (consultation, checkup, etc.)
                    Status = a.Status // Current appointment status (Pending)
                }).ToList()
            };

            // Pass the model to the view
            return View(model);
        }


        public async Task<IActionResult> AddNewAppointment()
        {
            // Create patient dropdown list
            var patientList = await _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Firstname + " " + p.Lastname
                })
                .ToListAsync();

            // Create staff dropdown list
            var staffList = await _context.ClinicalStaffs
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                })
                .ToListAsync();


            //Return the dta by viewbag
            ViewBag.PatientList = patientList;
            ViewBag.StaffList = staffList;
            return View();
        }

        //Helper for data extraction(this return thr query result)
        private async Task<List<Availability>> GetStaffAvailability(int clinicalStaffId, DateTime selectedDate)
        {
            return await _context.Availabilities
                           .Where(a => a.ClinicalStaffID == clinicalStaffId
                                       && (a.SlotDate == selectedDate ||
                                          (a.SlotDate == null && a.Day == selectedDate.DayOfWeek))
                                       && a.Status == AvailabilityStatus.Available.ToString())
                           .ToListAsync();
        }



        // Action to show Add New Appointment form
        [HttpPost]
        public async Task<IActionResult> GetDateAndTimeSlots(DateTime selectedDate, int StaffId)
        {
            var patients = await GetStaffAvailability(StaffId, selectedDate);

            var staffList = await _context.ClinicalStaffs
                .Include(s => s.Availabilities)
                .ToListAsync();

            var model = new NewAppointmentViewModel
            {
                ClinicalStaffs = staffList,
                SelectedDate = selectedDate
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Staff)
                .Include(a => a.Availability)
                .ToListAsync();

            var calendarData = appointments
                .Where(a => a.Availability != null && a.AppointmentDate != null)
                .Select(a =>
                {
                    // Use the actual appointment date
                    DateTime appointmentDate = a.AppointmentDate.Date;
                    DateTime startDateTime = appointmentDate + a.Availability.StartTime;
                    DateTime endDateTime = appointmentDate + a.Availability.EndTime;

                    return new
                    {
                        id = a.AppointmentId,
                        title = a.VisitType,
                        start = startDateTime.ToString("o"), // ISO 8601
                        end = endDateTime.ToString("o"),
                        color = a.Status == Enum.AppointmentEnum.Confirmed.ToString() ? "#CBE5DC" :
                                a.Status == Enum.AppointmentEnum.Completed.ToString() ? "#AEEBAB" :
                                "#FFD400",
                        extendedProps = new
                        {
                            visitType = a.VisitType,
                            status = a.Status,
                            patientName = a.Patient != null ? $"{a.Patient.Firstname} {a.Patient.Lastname}" : "",
                            doctorName = a.Staff != null ? $"{a.Staff.Firstname} {a.Staff.Lastname}" : "",
                            description = a.Description,
                            date = appointmentDate.ToString("yyyy-MM-dd"), // Include date separately
                            time = $"{startDateTime:HH:mm} - {endDateTime:HH:mm}"
                        }
                    };
                })
                .ToList();

            return Json(calendarData);
        }

        // Action to Confirm Appointment
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            // Load appointment with related Patient, Staff, and Availability
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Staff)
                .Include(a => a.Availability)
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
                // Log the error
                Console.WriteLine("Error confirming appointment: " + ex);
                // Optionally, you can return an error view/message here
                return StatusCode(500, "Internal server error while confirming appointment.");
            }

            // Reload all appointments with related entities
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Staff)
                .Include(a => a.Availability)
                .ToListAsync();

            // Build the ViewModel for Index
            var model = new AppointmentPageViewModel
            {
                // All appointments (used for calendar, etc.)
                Appointments = appointments,

                // Only "Pending" appointments for notification panel
                PendingAppointments = appointments
                    .Where(a => a.Status == Enum.AppointmentEnum.Pending.ToString())
                    .Select(a => new AppointmentPendingApprovalViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        PatientName = a.Patient != null
                            ? $"{a.Patient.Firstname} {a.Patient.Lastname}"
                            : "Unknown Patient",
                        DoctorName = a.Staff != null
                            ? $"{a.Staff.Firstname} {a.Staff.Lastname}"
                            : "Unknown Doctor",
                        VisitType = a.VisitType,
                        AppointmentDate = a.AppointmentDate,
                        Status = a.Status
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
                  // Include TimeSlot (for start/end times)
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
                        PatientName = $"{a.Patient.Firstname} {a.Patient.Lastname}",
                        DoctorName = $"{a.Staff.Firstname} {a.Staff.Lastname}",
                        VisitType = a.VisitType,
                        Status = a.Status
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
            //Extrct the appointment data and turn to list async


            var pending = await _context.Appointments
                .Where(a => a.Status == "Pending")
                .Select(a => new AppointmentPendingApprovalViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PatientName = $"{a.Patient.Firstname} {a.Patient.Lastname}",
                    DoctorName = $"{a.Staff.Firstname} {a.Staff.Lastname}",
                    VisitType = a.VisitType,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
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
                foreach (var state in ModelState)
                {
                    var key = state.Key;
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }

                ViewBag.Error = "Please correct the errors in the form!";
                return View(model);
            }

            // Fetch the template availability (recurring slot)
            var templateAvailability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == model.AvailabilityId && a.Status == AvailabilityStatus.Available.ToString());

            if (templateAvailability == null)
            {
                TempData["Error"] = "Selected availability not found or already booked!";
                return View(model);
            }

            // Create a new availability entry for the specific selected date
            var bookedAvailability = new Availability
            {
                ClinicalStaffID = templateAvailability.ClinicalStaffID,
                Day = templateAvailability.Day,
                StartTime = templateAvailability.StartTime,
                EndTime = templateAvailability.EndTime,
                Status = AvailabilityStatus.Unavailable.ToString(),
                SlotDate = model.SelectedDate // Only this date is booked
            };

            // Save the booked availability to generate Id
            await _context.Availabilities.AddAsync(bookedAvailability);
            await _context.SaveChangesAsync();

            // Create the appointment using the booked availability
            var appointment = new Appointment
            {
                PatientId = model.PatientId,
                ClinicalStaffID = model.ClinicalStaffID,
                AvailabilityId = bookedAvailability.AvailabilityId,
                VisitType = model.VisitType,
                Description = model.Description,
                Status = Enum.AppointmentEnum.Pending.ToString(),
                CreatedAt = DateTime.Now,
                AppointmentDate = model.SelectedDate
            };

            await _context.Appointments.AddAsync(appointment);

            // Optionally update patient status
            var patient = await _context.Patients.FindAsync(model.PatientId);
            if (patient != null)
            {
                patient.PatientStatus = Enum.PatientStatusEnum.PendingAssesment.ToString();
                _context.Update(patient);
            }

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Appointment scheduled successfully!";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index", "Appointment");
        }

        // POST: Submit date from calendar
        [HttpPost]
        public IActionResult SubmitDate(DateTime SelectedDate, int PatientId, int ClinicalStaffID)
        {
            ViewBag.SelectedDate = SelectedDate.ToString("yyyy-MM-dd");


            // Query availabilities for that day
            var availabilities = _context.Availabilities
                                .Where(a => a.ClinicalStaffID == ClinicalStaffID
                                            && (a.SlotDate == SelectedDate || (a.SlotDate == null && a.Day == SelectedDate.DayOfWeek))
                                            && a.Status == AvailabilityStatus.Available.ToString())
                                .ToList();
            // Display slots in console
            foreach (var a in availabilities)
            {
                Console.WriteLine($"AvailabilityId: {a.AvailabilityId}, Day: {a.Day}, StartTime: {a.StartTime:hh\\:mm}, EndTime: {a.EndTime:hh\\:mm}, Status: {a.Status}");
            }

            // Get patient and staff names
            var patient = _context.Patients
                                  .Where(p => p.PatientId == PatientId)
                                  .Select(p => new { p.Firstname, p.Lastname })
                                  .FirstOrDefault();

            var clinicalStaff = _context.ClinicalStaffs
                                        .Where(c => c.ClinicalStaffID == ClinicalStaffID)
                                        .Select(c => new { c.Firstname, c.Lastname })
                                        .FirstOrDefault();

            var model = new AppointmentViewModel
            {
                PatientId = PatientId,
                ClinicalStaffID = ClinicalStaffID,
                PatientName = patient != null ? $"{patient.Firstname} {patient.Lastname}" : "",
                ClinicalStaffName = clinicalStaff != null ? $"{clinicalStaff.Firstname} {clinicalStaff.Lastname}" : "",
                SelectedDate = SelectedDate
            };

            //Return the list of the Availabilities
            ViewBag.AvailableTimes = availabilities;

            return View("ScheduleAppointment", model);
        }


        // Action to mark Appointment as Completed
        [HttpPost]
        public async Task<IActionResult> Completed(int id)
        {
            // Find the appointment by ID
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found!";
                return RedirectToAction("Index"); // or return a suitable view
            }

            // Update status
            appointment.Status = Enum.AppointmentEnum.Completed.ToString();

            try
            {
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Appointment marked as completed!";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Failed to update appointment!";
            }

            return RedirectToAction("Index"); // Redirect back to appointments list
        }

      

      

        //public IActionResult NewAppointment()
        //{
        //    var availabilities = 
        //    return View();
        //}
    }
}