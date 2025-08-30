using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            // Fetch all booked appointments
            var appointments = await _context.NewAppointments
                .Include(a => a.ClinicalStaff) // doctor info
                .Include(a => a.Patient)       // patient info
                .Where(a => a.Status == Enum.AppointmentEnum.Pending.ToString()).ToListAsync();

            // Fetch patients who are waitlisted
            var waitlistedPatients = await _context.Patients
                .Where(p => p.PatientStatus == Enum.PatientStatusEnum.Waitlisted.ToString())
                .Select(p => new AddNewPatientViewModel
                {
                    PatientId = p.PatientId,
                    Firstname = p.Firstname,
                    Lastname = p.Lastname,
                    MiddleName = p.MiddleName,
                    ContactNumber = p.PhoneNumber,
                    Sex = p.Sex,
                    MaritalStatus = p.MaritalStatus,
                    DateOfBirth = p.DateOfBirth,
                    PhotoUrl = p.PhotoUrl,
                    // Address fields not in Patient model; set empty
                    House_Unit = string.Empty,
                    Street = string.Empty,
                    Subdivision_Village = string.Empty,
                    Barangay = string.Empty,
                    City = string.Empty,
                    Province = string.Empty,
                })
                .ToListAsync();

            var model = new AppointmentPageViewModel
            {
                Appointments = appointments,

                PendingAppointments = appointments.Select(a => new AppointmentPendingApprovalViewModel
                {
                    AppointmentId = a.AppointmentID,
                    PatientName = $"{a.Patient.Firstname} {a.Patient.Lastname}",
                    DoctorName = $"{a.ClinicalStaff.Firstname} {a.ClinicalStaff.Lastname}", // ✅ corrected property name
                    AppointmentDate = a.AppointmentDate,
                    VisitType = a.VisitType,
                    TimeSlot = $"{DateTime.Today.Add(a.TimeSlot).ToString("h:mm tt")} to " +
                          $"{DateTime.Today.Add(a.TimeSlot).AddHours(1).ToString("h:mm tt")}",
                    Status = a.Status
                }).ToList(),
                WaitlistedPatients = waitlistedPatients,
            };

            //Return all doctors TODO add more doctors
            ViewBag.Doctors = await _context.ClinicalStaffs
                                    .Where(a => a.Position == "Physician").ToListAsync();

            return View(model);
        }


        public async Task<IActionResult> AddNewAppointment(int? id)
        {
            var vm = new NewAppointmentViewModel();

            ViewBag.StaffList = await _context.ClinicalStaffs
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                })
                .ToListAsync();

            // Pre-populate patient list with optional preselection
            ViewBag.PatientList = await _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Firstname + " " + p.Lastname,
                    Selected = id.HasValue && p.PatientId == id.Value
                })
                .ToListAsync();

            // If coming from waitlisted quick action, preselect that patient
            if (id.HasValue && id > 0)
            {
                vm.PatientId = id.Value;
            }

            //Pass the ID
            ViewBag.PatientId = id;
            ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
            ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
            ViewBag.SelectedDate = null;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SelectDoctor(NewAppointmentViewModel model)
        {
            // If no doctor selected, just reload the form
            if (model.ClinicalStaffID <= 0)
            {
                ViewBag.StaffList = await _context.ClinicalStaffs
                    .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = s.ClinicalStaffID.ToString(),
                        Text = $"{s.Firstname} {s.Lastname}"
                    }).ToListAsync();

                // Preserve patient selection
                ViewBag.PatientList = await _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = p.Firstname + " " + p.Lastname,
                        Selected = model.PatientId > 0 && p.PatientId == model.PatientId
                    })
                    .ToListAsync();

                return View("AddNewAppointment", model);
            }

            // Put the selected doctor back into the model
            model.ClinicalStaffID = model.ClinicalStaffID;

            // Make sure PatientFullname is set again
            if (model.PatientId > 0)
            {
                var patient = await _context.Patients
                    .Where(p => p.PatientId == model.PatientId)
                    .Select(p => new { p.Firstname, p.Lastname })
                    .FirstOrDefaultAsync();

                if (patient != null)
                    model.PatientFullname = $"{patient.Firstname} {patient.Lastname}";
            }


            // Fill doctor list again
            ViewBag.StaffList = await _context.ClinicalStaffs
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = $"{s.Firstname} {s.Lastname}"
                }).ToListAsync();

            // Preserve patient selection
            ViewBag.PatientList = await _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Firstname + " " + p.Lastname,
                    Selected = model.PatientId > 0 && p.PatientId == model.PatientId
                })
                .ToListAsync();

            return View("AddNewAppointment", model);
        }


        // POST: user picked a date (after picking a doctor)
        [HttpPost]
        public async Task<IActionResult> GetDateAndTimeSlots(NewAppointmentViewModel model)
        {
            // Re-fill staff dropdown
            ViewBag.StaffList = await _context.ClinicalStaffs
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = (s.Firstname + " " + s.Lastname)
                })
                .ToListAsync();

            if (model.ClinicalStaffID == null || model.ClinicalStaffID <= 0 || model.SelectedDate == null)
            {
                ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
                ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
                ViewBag.SelectedDate = model.SelectedDate;
                
                // Preserve patient list even when no date is selected
                ViewBag.PatientList = await _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = p.Firstname + " " + p.Lastname,
                        Selected = model.PatientId > 0 && p.PatientId == model.PatientId
                    })
                    .ToListAsync();
                
                return View("AddNewAppointment", model);
            }

            var date = model.SelectedDate.Date;
            var dow = date.DayOfWeek;

            // 1. Get recurring or date-specific availability
            var slots = await _context.Availabilities
                .Where(a => a.ClinicalStaffID == model.ClinicalStaffID
                            && (
                                  a.SlotDate == date
                                  || (a.SlotDate == null && a.Day == dow)
                               )
                            && a.Status == Enum.AvailabilityStatus.Available.ToString())
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            // 2. Get all occupied appointments (Pending, Confirmed, Booked)
            var takenAppointments = await _context.NewAppointments
                .Where(appt => appt.ClinicalStaffID == model.ClinicalStaffID
                               && appt.AppointmentDate == date
                               && (appt.Status == Enum.AppointmentEnum.Pending.ToString()
                                   || appt.Status == Enum.AppointmentEnum.Confirmed.ToString()
                                   || appt.Status == Enum.AppointmentEnum.Booked.ToString()))
                .ToListAsync();

            // 3. Filter out slots that overlap
            var freeSlots = slots
                .Where(slot =>
                    !takenAppointments.Any(appt =>
                        appt.TimeSlot >= slot.StartTime &&
                        appt.TimeSlot < slot.EndTime   // falls inside this availability
                    )
                )
                .ToList();

            foreach(var s in freeSlots)
            {
                Console.WriteLine(freeSlots);
            }

            // ✅ Preserve patient fullname
            if (model.PatientId > 0)
            {
                var patient = await _context.Patients
                    .Where(p => p.PatientId == model.PatientId)
                    .Select(p => new { p.Firstname, p.Lastname })
                    .FirstOrDefaultAsync();

                if (patient != null)
                    model.PatientFullname = $"{patient.Firstname} {patient.Lastname}";
            }

            // Patient dropdown - preserve selection
            ViewBag.PatientList = await _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Firstname + " " + p.Lastname,
                    Selected = model.PatientId > 0 && p.PatientId == model.PatientId
                })
                .ToListAsync();

            ViewBag.AvailableTimes = freeSlots;
            ViewBag.SelectedDate = date.ToString("yyyy-MM-dd");

            // Generate timeslot dropdown
            ViewBag.TimeSlotList = freeSlots.Select(s => new SelectListItem
            {
                Value = s.StartTime.ToString(),
                Text = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}"
            }).ToList();

            return View("AddNewAppointment", model);
        }


        // POST: final submit
        [HttpPost]
        public async Task<IActionResult> AddNewAppointment(NewAppointmentViewModel model)
        {
            //Check model State
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
                // Fill doctor list again
                ViewBag.StaffList = await _context.ClinicalStaffs
                    .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = s.ClinicalStaffID.ToString(),
                        Text = $"{s.Firstname} {s.Lastname}"
                    }).ToListAsync();


                ViewBag.PatientList = await _context.Patients
                                            .Select(p => new SelectListItem
                                            {
                                                Value = p.PatientId.ToString(),
                                                Text = p.Firstname + " " + p.Lastname
                                            })
                                             .ToListAsync();
                return View(model);
            }

            // Find the exact availability slot that was booked
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.ClinicalStaffID == model.ClinicalStaffID
                                       && a.StartTime == model.TimeSlot
                                       && (a.SlotDate == model.SelectedDate || (a.SlotDate == null && a.Day == model.SelectedDate.DayOfWeek)));

            //Populate Model
            var NewAppointment = new NewAppointment
            {
                AppointmentID = model.AppointmentID,
                ClinicalStaffID = model.ClinicalStaffID,
                PatientId = model.PatientId,
                TimeSlot = model.TimeSlot,
                AppointmentDate = model.SelectedDate,
                VisitType = model.VisitType,
                Status = Enum.AppointmentEnum.Pending.ToString(),
                Description = model.Description
            };

            //Find Patient
            var patient = await _context.Patients.FirstOrDefaultAsync(s => s.PatientId == model.PatientId);

            try
            {
                //Save model to database 
                await _context.NewAppointments.AddAsync(NewAppointment);
                await _context.SaveChangesAsync();

                if (NewAppointment.VisitType == "Medical Assessment")
                {
                    patient.PatientStatus = Enum.PatientStatusEnum.PendingAssessment.ToString();
                    _context.Patients.Update(patient);
                    await _context.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            // Return the Index view with updated model
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.NewAppointments
                .Include(a => a.Patient)
                .Include(a => a.ClinicalStaff)
                .Where(a => a.Status != Enum.AppointmentEnum.Pending.ToString()) // ✅ exclude Pending
                .ToListAsync();

            var calendarData = appointments
                .Where(a => a.AppointmentDate != null)
                .Select(a =>
                {
                    DateTime appointmentDate = a.AppointmentDate.Date;

                    // TimeSlot is TimeSpan (start time of appointment)
                    DateTime startDateTime = appointmentDate + a.TimeSlot;

                    // Assume duration = 1 hour
                    DateTime endDateTime = startDateTime.AddHours(1);

                    // Assign color based on status
                    string color = a.Status switch
                    {
                        "Booked" => "#CBE5DC",
                        "Cancelled" => "#F8D7DA",
                        "Completed" => "#D1E7DD",
                        _ => "#E2E3E5" // default
                    };

                    return new
                    {
                        id = a.AppointmentID,
                        title = a.VisitType,
                        start = startDateTime.ToString("o"),
                        end = endDateTime.ToString("o"),
                        color = color,
                        extendedProps = new
                        {
                            doctorId = a.ClinicalStaffID,
                            visitType = a.VisitType,
                            patientId = a.PatientId,
                            status = a.Status,
                            patientName = a.Patient != null ? $"{a.Patient.Firstname} {a.Patient.Lastname}" : "",
                            doctorName = a.ClinicalStaff != null ? $"{a.ClinicalStaff.Firstname} {a.ClinicalStaff.Lastname}" : "",
                            description = a.Description,
                            date = appointmentDate.ToString("yyyy-MM-dd"),
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
            var appointment = await _context.NewAppointments
                .Include(a => a.Patient)
                .Include(a => a.ClinicalStaff)
                .FirstOrDefaultAsync(a => a.AppointmentID == id);

            // If no appointment found, return 404
            if (appointment == null)
            {
                return NotFound();
            }

            try
            {
                // Update the status to "Confirmed"
                appointment.Status = Enum.AppointmentEnum.Booked.ToString();

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
            var appointments = await _context.NewAppointments
                .Include(a => a.Patient)
                .Include(a => a.ClinicalStaff)
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
                        AppointmentId = a.AppointmentID,
                        PatientName = a.Patient != null
                            ? $"{a.Patient.Firstname} {a.Patient.Lastname}"
                            : "Unknown Patient",
                        DoctorName = a.ClinicalStaff != null
                            ? $"{a.ClinicalStaff.Firstname} {a.ClinicalStaff.Lastname}"
                            : "Unknown Doctor",
                        VisitType = a.VisitType,
                        TimeSlot = $"{DateTime.Today.Add(a.TimeSlot).ToString("h:mm tt")} to " +
                                    $"{DateTime.Today.Add(a.TimeSlot).AddHours(1).ToString("h:mm tt")}",
                        AppointmentDate = a.AppointmentDate,
                        Status = a.Status
                    })
                    .ToList()
            };

            // Return the Index view with updated model
            return RedirectToAction("Index");
        }


        // Action to Cancel Appointment
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            // Find the appointment by ID, include related Day and TimeSlot
            var appointment = await _context.NewAppointments
                  // Include TimeSlot (for start/end times)
                .FirstOrDefaultAsync(a => a.AppointmentID == id);

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
            var appointments = await _context.NewAppointments
            
                .ToListAsync();

            // Build the updated ViewModel
            var model = new AppointmentPageViewModel
            {
                Appointments = appointments,
                PendingAppointments = appointments
                    .Where(a => a.Status == Enum.AppointmentEnum.Pending.ToString()) // still only pending
                    .Select(a => new AppointmentPendingApprovalViewModel
                    {
                        AppointmentId = a.AppointmentID,
                        PatientName = $"{a.Patient.Firstname} {a.Patient.Lastname}",
                        DoctorName = $"{a.ClinicalStaff.Firstname} {a.ClinicalStaff.Lastname}",
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


            var pending = await _context.NewAppointments
                .Where(a => a.Status == "Booked")
                .Select(a => new AppointmentPendingApprovalViewModel
                {
                    AppointmentId = a.AppointmentID,
                    PatientName = $"{a.Patient.Firstname} {a.Patient.Lastname}",
                    DoctorName = $"{a.ClinicalStaff.Firstname} {a.ClinicalStaff.Lastname}",
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
        public async Task<IActionResult> ScheduleAppointment(NewAppointmentViewModel model)
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

            // Ensure staff exists
            var staffExists = await _context.ClinicalStaffs
                .AnyAsync(s => s.ClinicalStaffID == model.ClinicalStaffID);

            if (!staffExists)
            {
                TempData["Error"] = "Selected doctor not found!";
                return View(model);
            }

            // Check if slot is already booked for this doctor on this date
            var slotTaken = await _context.NewAppointments.AnyAsync(a =>
                a.ClinicalStaffID == model.ClinicalStaffID &&
                a.AppointmentDate == model.SelectedDate.Date &&
                a.TimeSlot == model.TimeSlot &&
                (a.Status == Enum.AppointmentEnum.Pending.ToString() ||
                 a.Status == Enum.AppointmentEnum.Confirmed.ToString())
            );

            if (slotTaken)
            {
                TempData["Error"] = "This slot is already booked!";
                return View(model);
            }

            // Create a new appointment
            var appointment = new NewAppointment
            {
                PatientId = model.PatientId,
                ClinicalStaffID = model.ClinicalStaffID,
                AppointmentDate = model.SelectedDate.Date,
                TimeSlot = model.TimeSlot, // ✅ TimeSpan
                VisitType = model.VisitType,
                Description = model.Description,
                Status = Enum.AppointmentEnum.Confirmed.ToString(),
            };

            await _context.NewAppointments.AddAsync(appointment);

            // Optionally update patient status
            var patient = await _context.Patients.FindAsync(model.PatientId);
            if (patient != null)
            {
                patient.PatientStatus = Enum.PatientStatusEnum.PendingAssessment.ToString();
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

            // Get all availabilities for that doctor on the selected day
            var availabilities = _context.Availabilities
                .Where(a => a.ClinicalStaffID == ClinicalStaffID
                            && (a.SlotDate == SelectedDate || (a.SlotDate == null && a.Day == SelectedDate.DayOfWeek))
                            && a.Status == AvailabilityStatus.Available.ToString())
                .ToList();

            // Get booked appointments for that date
            var bookedAppointments = _context.NewAppointments
                .Where(appt => appt.ClinicalStaffID == ClinicalStaffID
                               && appt.AppointmentDate == SelectedDate
                               && appt.Status == "Booked")
                .Select(appt => appt.TimeSlot)
                .ToList();

            // Filter out booked slots
            var freeAvailabilities = availabilities
                .Where(a => !bookedAppointments.Contains(a.StartTime))
                .ToList();

            ViewBag.AvailableTimes = freeAvailabilities;

            // ✅ Fetch names again
            var patient = _context.Patients.FirstOrDefault(p => p.PatientId == PatientId);
            var doctor = _context.ClinicalStaffs.FirstOrDefault(d => d.ClinicalStaffID == ClinicalStaffID);

            var model = new AppointmentViewModel
            {
                PatientId = PatientId,
                ClinicalStaffID = ClinicalStaffID,
                SelectedDate = SelectedDate,
                PatientName = patient != null ? $"{patient.Firstname} {patient.Lastname}" : "",
                ClinicalStaffName = doctor != null ? $"{doctor.Firstname} {doctor.Lastname}" : ""
            };

            return View("ScheduleAppointment", model);
        }



        // Action to mark Appointment as Completed
        [HttpPost]
        public async Task<IActionResult> Completed(int id, int patientId)
        {
            // Find the appointment by ID
            var appointment = await _context.NewAppointments.FindAsync(id);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found!";
                return RedirectToAction("Index"); // or return a suitable view
            }

            // Update status
            appointment.Status = Enum.AppointmentEnum.Completed.ToString();

            try
            {
                _context.NewAppointments.Update(appointment);

                // Update patient status
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient != null)
                {
                    patient.PatientStatus = Enum.PatientStatusEnum.PendingReview.ToString(); // or use an Enum like PatientStatusEnum
                    _context.Patients.Update(patient);
                }

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