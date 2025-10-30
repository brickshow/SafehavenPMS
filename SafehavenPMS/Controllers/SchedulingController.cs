using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafehavenPMS.ViewModel;
using System.Reflection.Metadata.Ecma335;
using SafehavenPMS.Services; // <-- added

namespace SafehavenPMS.Controllers
{
    [Authorize]
    public class SchedulingController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly ActivityLogService _activityService; // <-- added

        private static string FullName(Patient p) => p == null ? "" : $"{p.Firstname} {p.Lastname}"; // <-- added
        private static string StaffName(ClinicalStaff s) => s == null ? "" : $"{s.Firstname} {s.Lastname}"; // <-- added

        public SchedulingController(SafehavenPMSContext context, ActivityLogService activityService) // <-- modified
        {
            _context = context;
            _activityService = activityService;
        }
        public async Task<IActionResult> Index(
                   int? page = 1,
                   int? pageSize = 10,
                   string searchQuery = null,
                   string status = null,
                   string sortOrder = null,
                   string sortBy = null)
        {
            // Query appointments with patient and staff information
            var query = _context.NewAppointments
                .Include(a => a.Patient)
                .Include(a => a.ClinicalStaff)
                // only show schedules whose status is NOT "Completed"
                .Where(a => a.Status != Enum.AppointmentEnum.Completed.ToString())
                .AsQueryable();

            // Get waitlisted count (appointments with Waitlisted status)
            ViewBag.WaitlistedCount = await _context.NewAppointments
                .CountAsync(p => p.Status == Enum.AppointmentEnum.Pending.ToString());

            //Total count for psychiatric 
            ViewBag.PsychiatricQueue = await _context.PsychiatricAssessments
                .CountAsync(p => p.Status == PatientStatusEnum.Admitted.ToString());

            // Pass current filters/sorting to view
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;
            ViewBag.SortBy = sortBy ?? "";

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(a =>
                    a.Patient.Firstname.ToLower().Contains(searchQuery) ||
                    a.Patient.Lastname.ToLower().Contains(searchQuery) ||
                    a.PatientId.ToString().Contains(searchQuery));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy) && sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(a => a.Patient.Firstname).ThenBy(a => a.Patient.Lastname)
                    : query.OrderByDescending(a => a.Patient.Firstname).ThenByDescending(a => a.Patient.Lastname);
            }
            else if (!string.IsNullOrEmpty(sortBy) && sortBy.Equals("ScheduledDate", StringComparison.OrdinalIgnoreCase))
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(a => a.ScheduleDate)
                    : query.OrderByDescending(a => a.ScheduleDate);
            }
            else if (!string.IsNullOrEmpty(sortBy) && sortBy.Equals("Status", StringComparison.OrdinalIgnoreCase))
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(a => a.Status)
                    : query.OrderByDescending(a => a.Status);
            }
            else
            {
                // Default sorting by creation date
                query = sortOrder == "ascending"
                    ? query.OrderBy(a => a.CreatedAt)
                    : query.OrderByDescending(a => a.CreatedAt);
            }

            // Get total count for pagination
            int totalItems = await query.CountAsync();
            ViewBag.TotalPatientCount = totalItems;

            // Apply pagination
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var appointments = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            // Map to view model
            var schedulingVM = appointments.Select(a => new SchedulingViewModel
            {
                ScheduleId = a.ScheduleId,
                PatientId = a.PatientId,
                ClinicalStaffID = a.ClinicalStaffID,
                Type = a.Type,
                ScheduleDate = a.ScheduleDate,
                ScheduleTime = a.ScheduleTime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                PatientName = a.Patient != null ? $"{a.Patient.Firstname} {a.Patient.Lastname}" : "Unknown Patient",
                ClinicalStaffName = a.ClinicalStaff != null ? $"{a.ClinicalStaff.Firstname} {a.ClinicalStaff.Lastname}" : "-"
            }).ToList();

            return View(schedulingVM);
        }

        [HttpGet]
        public IActionResult SortBy(string sortBy, string sortOrder, string searchQuery, string status, int page = 1, int pageSize = 10)
        {
            return RedirectToAction("Index", new { sortBy, sortOrder, searchQuery, status, page, pageSize });
        }

        [HttpGet]
        public IActionResult Search(string searchQuery, string status, string sortBy, string sortOrder, int pageSize = 10)
        {
            return RedirectToAction("Index", new
            {
                searchQuery,
                page = 1,
                pageSize,
                status,
                sortBy,
                sortOrder
            });
        }

        public async Task<IActionResult> ScheduleAppointment(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "No patient selected.";
                return RedirectToAction("Index");
            }

            var vm = new ScheduleAppointmentViewModel();
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id.Value);

            // Pre-populate from latest NewAppointment if exists
            var latestAppt = await _context.NewAppointments
                .Where(a => a.PatientId == id && a.Status != "Completed")
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (patient != null)
            {
                vm.PatientId = patient.PatientId;
                vm.PatientName = $"{patient.Firstname} {patient.Lastname}";
            }

            if (latestAppt != null)
            {
                vm.ClinicalStaffID = latestAppt.ClinicalStaffID ?? 0;
                vm.SelectedDate = latestAppt.ScheduleDate ?? DateTime.Today;
                vm.TimeSlot = latestAppt.ScheduleTime;
                vm.VisitType = latestAppt.Type;
                vm.Description = latestAppt.Notes;
            }

            // Only show psychiatrists if type is PsychiatricAssesment
            if (string.Equals(vm.VisitType, "PsychiatricAssesment", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.StaffList = await _context.ClinicalStaffs
                    .Where(s => s.Position == "Psychiatrist")
                    .Select(s => new SelectListItem
                    {
                        Value = s.ClinicalStaffID.ToString(),
                        Text = s.Firstname + " " + s.Lastname
                    })
                    .ToListAsync();
            }
            else
            {
                ViewBag.StaffList = await _context.ClinicalStaffs
                    .Where(s => s.Position == "Physician")
                    .Select(s => new SelectListItem
                    {
                        Value = s.ClinicalStaffID.ToString(),
                        Text = s.Firstname + " " + s.Lastname
                    })
                    .ToListAsync();
            }

            ViewBag.PatientId = id;
            ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
            ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
            ViewBag.SelectedDate = vm.SelectedDate;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SelectDoctor(ScheduleAppointmentViewModel model)
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

                return View("ScheduleAppointment", model);
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
                    model.PatientName = $"{patient.Firstname} {patient.Lastname}";
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

            return View("ScheduleAppointment", model);
        }


        // POST: user picked a date (after picking a doctor)
        [HttpPost]
        public async Task<IActionResult> GetDateAndTimeSlots(ScheduleAppointmentViewModel model)
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

                return View("ScheduleAppointment", model);
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
                            && appt.ScheduleDate == date
                            && (appt.Status == Enum.AppointmentEnum.Pending.ToString()
                                || appt.Status == Enum.AppointmentEnum.Confirmed.ToString()
                                || appt.Status == Enum.AppointmentEnum.Booked.ToString()))
                .ToListAsync();

            // 3. Filter out slots that overlap
            var freeSlots = slots
                .Where(slot =>
                    !takenAppointments.Any(appt =>
                        TimeSpan.Parse(appt.ScheduleTime) >= slot.StartTime &&
                        TimeSpan.Parse(appt.ScheduleTime) < slot.EndTime   // falls inside this availability
                    )
                )
                .ToList();

            foreach (var s in freeSlots)
            {
                Console.WriteLine(freeSlots);
            }

            //  Preserve patient fullname
            if (model.PatientId > 0)
            {
                var patient = await _context.Patients
                    .Where(p => p.PatientId == model.PatientId)
                    .Select(p => new { p.Firstname, p.Lastname })
                    .FirstOrDefaultAsync();

                if (patient != null)
                    model.PatientName = $"{patient.Firstname} {patient.Lastname}";
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

            return View("ScheduleAppointment", model);
        }


        // POST: final submit
        [HttpPost]
        public async Task<IActionResult> ScheduleAppointment(ScheduleAppointmentViewModel model)
        {
            // Validate required fields
            if (model.ClinicalStaffID <= 0)
            {
                ModelState.AddModelError("ClinicalStaffID", "Please select a doctor");
            }
            if (string.IsNullOrEmpty(model.TimeSlot))
            {
                ModelState.AddModelError("TimeSlot", "Please select a time slot");
            }
            if (model.SelectedDate == default(DateTime))
            {
                ModelState.AddModelError("SelectedDate", "Please select a date");
            }
            if (string.IsNullOrEmpty(model.VisitType))
            {
                ModelState.AddModelError("VisitType", "Please select a visit type");
            }

            //Check model State
            if (!ModelState.IsValid)
            {
                // Log validation errors
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
                    .Where(s => s.Position == "Physician")
                    .Select(s => new SelectListItem
                    {
                        Value = s.ClinicalStaffID.ToString(),
                        Text = $"{s.Firstname} {s.Lastname}"
                    }).ToListAsync();


                // Get available time slots for the selected date
                if (model.SelectedDate != default(DateTime) && model.ClinicalStaffID > 0)
                {
                    var date = model.SelectedDate.Date;
                    var dow = date.DayOfWeek;

                    var slots = await _context.Availabilities
                        .Where(a => a.ClinicalStaffID == model.ClinicalStaffID
                                  && (a.SlotDate == date || (a.SlotDate == null && a.Day == dow))
                                  && a.Status == AvailabilityStatus.Available.ToString())
                        .OrderBy(a => a.StartTime)
                        .ToListAsync();

                    ViewBag.AvailableTimes = slots;
                    ViewBag.TimeSlotList = slots.Select(s => new SelectListItem
                    {
                        Value = s.StartTime.ToString(),
                        Text = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}",
                        Selected = s.StartTime.ToString() == model.TimeSlot
                    }).ToList();
                }
                else
                {
                    ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
                    ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
                }

                ViewBag.SelectedDate = model.SelectedDate;
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                TempData["SuccessMessage"] = "Please fill in all required fields.";
                return View(model);
            }

            // Find the exact availability slot that was booked
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.ClinicalStaffID == model.ClinicalStaffID
                                    && a.StartTime == TimeSpan.Parse(model.TimeSlot)
                                    && (a.SlotDate == model.SelectedDate || (a.SlotDate == null && a.Day == model.SelectedDate.DayOfWeek)));

            if (availability != null)
            {
                // Mark the availability slot as scheduled
                availability.Status = AvailabilityStatus.Scheduled.ToString();
                _context.Availabilities.Update(availability);
            }

            // Set required fields
            model.Status = SchedulingStatus.Scheduled.ToString();  // Set status to Scheduled

            // Get ClinicalStaffName
            var doctor = await _context.ClinicalStaffs
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == model.ClinicalStaffID);
            if (doctor != null)
            {
                model.ClinicalStaffName = $"{doctor.Firstname} {doctor.Lastname}";
            }

            // Find existing appointment for this patient
            var existingAppointment = await _context.NewAppointments
                .FirstOrDefaultAsync(a => a.PatientId == model.PatientId);

            if (existingAppointment != null)
            {
                // Update existing appointment
                existingAppointment.ClinicalStaffID = model.ClinicalStaffID;
                existingAppointment.ScheduleTime = model.TimeSlot;
                existingAppointment.ScheduleDate = model.SelectedDate;
                existingAppointment.Type = model.VisitType;
                existingAppointment.Status = SchedulingStatus.Scheduled.ToString();
                existingAppointment.Notes = model.Description;
            }
            else
            {
                // If no existing appointment, create new one
                existingAppointment = new NewAppointment
                {
                    ClinicalStaffID = model.ClinicalStaffID,
                    PatientId = model.PatientId,
                    ScheduleTime = model.TimeSlot,
                    ScheduleDate = model.SelectedDate,
                    Type = model.VisitType,
                    Status = SchedulingStatus.Scheduled.ToString(),
                    Notes = model.Description
                };
                _context.NewAppointments.Add(existingAppointment);
            }

            //Find Patient
            var patient = await _context.Patients.FirstOrDefaultAsync(s => s.PatientId == model.PatientId);

            // Check if there's already a doctor assignment
            var doctorAssignment = await _context.ClinicalStaffPatients
                .FirstOrDefaultAsync(csp => csp.PatientId == model.PatientId);

            if (doctorAssignment != null)
            {
                // Update existing doctor assignment if different
                if (doctorAssignment.ClinicalStaffId != model.ClinicalStaffID)
                {
                    doctorAssignment.ClinicalStaffId = model.ClinicalStaffID ?? 0;
                    _context.ClinicalStaffPatients.Update(doctorAssignment);
                }
            }
            else
            {
                // Create new doctor assignment
                var newAssignment = new ClinicalStaffPatient
                {
                    ClinicalStaffId = model.ClinicalStaffID ?? 0,
                    PatientId = model.PatientId
                };
                _context.ClinicalStaffPatients.Add(newAssignment);
            }

            try
            {
                await _context.SaveChangesAsync();

                if (existingAppointment.Type == "Initial Assessment" && patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.PendingAssessment.ToString();
                    _context.Patients.Update(patient);
                    await _context.SaveChangesAsync();
                }

                // --- added activity log + notification ---
                var user = User?.Identity?.Name ?? "System";
                var patientName = FullName(patient);
                var doctorName = StaffName(doctor);
                await _activityService.LogAsync(
                    user,
                    "Scheduled appointment",
                    $"{existingAppointment.Type} for {patientName}" +
                        (string.IsNullOrWhiteSpace(doctorName) ? "" : $" with {doctorName}") +
                        $" on {existingAppointment.ScheduleDate:yyyy-MM-dd} {(existingAppointment.ScheduleTime ?? "(All Day)")}",
                    "Appointment",
                    "Info",
                    existingAppointment.PatientId);

                await _activityService.NotifyAsync(
                    user,
                    $"Scheduled {existingAppointment.Type} for {patientName} {existingAppointment.ScheduleDate:yyyy-MM-dd}",
                    type: "Success");
                // --- end added ---
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                ModelState.AddModelError("", "Failed to save appointment: " + ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // Small helper timeslot DTO used for viewbag
        private class TimeSlotDto
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }

        // GET: Scheduling/RescheduleAppointment/5
        [HttpGet]
        public async Task<IActionResult> RescheduleAppointment(int id)
        {
            // Load appointment with patient and staff
            var appt = await _context.NewAppointments
                .Include(a => a.Patient)
                .Include(a => a.ClinicalStaff)
                .FirstOrDefaultAsync(a => a.ScheduleId == id);

            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Index");
            }

            // Build viewmodel (assumes ScheduleAppointmentViewModel has these properties)
            var vm = new SafehavenPMS.ViewModel.ScheduleAppointmentViewModel
            {
                ScheduleId = appt.ScheduleId,
                PatientId = appt.PatientId,
                PatientName = appt.Patient != null ? $"{appt.Patient.Firstname} {appt.Patient.Lastname}" : "Unknown",
                ClinicalStaffID = appt.ClinicalStaffID ?? 0,
                SelectedDate = appt.ScheduleDate ?? DateTime.Today,
                TimeSlot = appt.ScheduleTime, // e.g. "09:00"
                VisitType = appt.Type,
                Description = appt.Notes
            };

            // Populate doctor list for select
            var staffList = await _context.ClinicalStaffs
                .Select(s => new { s.ClinicalStaffID, FullName = (s.Firstname + " " + s.Lastname + " - " + s.Position) })
                .ToListAsync();

            ViewBag.StaffList = new SelectList(staffList, "ClinicalStaffID", "FullName", vm.ClinicalStaffID);

            // Compute available times for the appointment date and selected staff
            DateTime date = appt.ScheduleDate?.Date ?? DateTime.Today;

            return View("RescheduleAppointment", vm);
        }

        // POST: Scheduling/RescheduleAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleAppointment(SafehavenPMS.ViewModel.ScheduleAppointmentViewModel model)
        {
            if (model == null || model.ScheduleId <= 0)
            {
                TempData["Error"] = "Invalid request.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("RescheduleAppointment", new { id = model.ScheduleId });
            }

            var appt = await _context.NewAppointments.FirstOrDefaultAsync(a => a.ScheduleId == model.ScheduleId);
            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Parse date + timeslot
                DateTime parsedDate;
                if (!DateTime.TryParse(model.SelectedDate.ToString("yyyy-MM-dd"), out parsedDate))
                {
                    TempData["Error"] = "Invalid date selected.";
                    return RedirectToAction("RescheduleAppointment", new { id = model.ScheduleId });
                }

                // Validate timeslot format "hh:mm"
                if (string.IsNullOrWhiteSpace(model.TimeSlot))
                {
                    TempData["Error"] = "Please select a timeslot.";
                    return RedirectToAction("RescheduleAppointment", new { id = model.ScheduleId });
                }

                // Check slot availability (allow current appointment to keep its slot)
                var conflict = await _context.NewAppointments.AnyAsync(a =>
                    a.ClinicalStaffID == model.ClinicalStaffID &&
                    a.ScheduleDate.HasValue &&
                    a.ScheduleDate.Value.Date == parsedDate.Date &&
                    a.ScheduleTime == model.TimeSlot &&
                    a.ScheduleId != model.ScheduleId);

                if (conflict)
                {
                    TempData["Error"] = "Selected timeslot is no longer available. Please choose another.";
                    return RedirectToAction("RescheduleAppointment", new { id = model.ScheduleId });
                }

                // Update appointment
                appt.ClinicalStaffID = model.ClinicalStaffID > 0 ? model.ClinicalStaffID : appt.ClinicalStaffID;
                appt.ScheduleDate = parsedDate;
                appt.ScheduleTime = model.TimeSlot;
                appt.Type = model.VisitType ?? appt.Type;
                appt.Notes = model.Description ?? appt.Notes;

                _context.NewAppointments.Update(appt);
                await _context.SaveChangesAsync();

                // --- added activity log + notification ---
                var pat = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == appt.PatientId);
                var doc = await _context.ClinicalStaffs.FirstOrDefaultAsync(c => c.ClinicalStaffID == appt.ClinicalStaffID);
                var user = User?.Identity?.Name ?? "System";
                await _activityService.LogAsync(
                    user,
                    "Rescheduled appointment",
                    $"{appt.Type} for {FullName(pat)} to {appt.ScheduleDate:yyyy-MM-dd} {appt.ScheduleTime ?? "(All Day)"}",
                    "Appointment",
                    "Info",
                    appt.PatientId);

                await _activityService.NotifyAsync(
                    user,
                    $"Rescheduled {appt.Type} for {FullName(pat)}",
                    type: "Warning");
                // --- end added ---

                TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while rescheduling the appointment.";
                return RedirectToAction("RescheduleAppointment", new { id = model.ScheduleId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int scheduleId)
        {
            if (scheduleId <= 0)
            {
                TempData["Error"] = "Invalid appointment id.";
                return RedirectToAction("Index");
            }

            var appt = await _context.NewAppointments
                .FirstOrDefaultAsync(a => a.ScheduleId == scheduleId);

            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // mark appointment cancelled
                appt.Status = Enum.AppointmentEnum.Cancelled.ToString();
                _context.NewAppointments.Update(appt);

                // try to restore availability slot if one exists (match by staff, date and start time)
                if (appt.ClinicalStaffID.HasValue && appt.ScheduleDate.HasValue && !string.IsNullOrWhiteSpace(appt.ScheduleTime))
                {
                    TimeSpan parsed;
                    if (TimeSpan.TryParse(appt.ScheduleTime, out parsed))
                    {
                        var avail = await _context.Availabilities.FirstOrDefaultAsync(a =>
                            a.ClinicalStaffID == appt.ClinicalStaffID &&
                            a.SlotDate.HasValue && a.SlotDate.Value.Date == appt.ScheduleDate.Value.Date &&
                            a.StartTime == parsed &&
                            a.Status == Enum.AvailabilityStatus.Scheduled.ToString());

                        if (avail != null)
                        {
                            avail.Status = Enum.AvailabilityStatus.Available.ToString();
                            _context.Availabilities.Update(avail);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // --- added activity log + notification ---
                var pat = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == appt.PatientId);
                var user = User?.Identity?.Name ?? "System";
                await _activityService.LogAsync(
                    user,
                    "Cancelled appointment",
                    $"{appt.Type} for {FullName(pat)} scheduled {appt.ScheduleDate:yyyy-MM-dd} {appt.ScheduleTime}",
                    "Appointment",
                    "Info",
                    appt.PatientId);

                await _activityService.NotifyAsync(
                    user,
                    $"Cancelled {appt.Type} for {FullName(pat)}",
                    type: "Info");
                // --- end added ---

                TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            }
            catch (Exception ex)
            {
                // optionally log ex
                TempData["Error"] = "Failed to cancel appointment.";
            }

            return RedirectToAction("Index");
        }

        // GET: Scheduling/PsySchedule/5
        [HttpGet]
        public async Task<IActionResult> PsySchedule(int? id = null)
        {
            if (id == null)
                return View();

            var vm = new ScheduleAppointmentViewModel();
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == id.Value);

            // Pre-populate from latest Psychiatric Assessment appointment if exists
            var latestPsyAppt = await _context.NewAppointments
                .Where(a => a.PatientId == id && a.Type == "Psychiatric Assessment")
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (patient != null)
            {
                vm.PatientId = patient.PatientId;
                vm.PatientName = $"{patient.Firstname} {patient.Lastname}";
            }

            if (latestPsyAppt != null)
            {
                vm.ClinicalStaffID = latestPsyAppt.ClinicalStaffID ?? 0;
                vm.SelectedDate = latestPsyAppt.ScheduleDate ?? DateTime.Today;
                vm.TimeSlot = latestPsyAppt.ScheduleTime;
                vm.VisitType = latestPsyAppt.Type;
                vm.Description = latestPsyAppt.Notes;
            }
            else
            {
                vm.VisitType = "Psychiatric Assessment";
                vm.SelectedDate = DateTime.Today;
            }

            // Only show psychiatrists in the staff list
            ViewBag.StaffList = await _context.ClinicalStaffs
                .Where(s => s.Position == "Psychiatrist")
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                })
                .ToListAsync();

            ViewBag.PatientId = id;
            ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
            ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
            ViewBag.SelectedDate = vm.SelectedDate;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PsySchedule(ScheduleAppointmentViewModel model)
        {
            // Validate required fields
            if (model.ClinicalStaffID <= 0)
            {
                ModelState.AddModelError("ClinicalStaffID", "Please select a psychiatrist");
            }
            if (string.IsNullOrEmpty(model.TimeSlot))
            {
                ModelState.AddModelError("TimeSlot", "Please select a time slot");
            }
            if (model.SelectedDate == default(DateTime))
            {
                ModelState.AddModelError("SelectedDate", "Please select a date");
            }
            if (string.IsNullOrEmpty(model.VisitType))
            {
                ModelState.AddModelError("VisitType", "Please select a visit type");
            }

            if (!ModelState.IsValid)
            {
                // Only show psychiatrists in the staff list
                ViewBag.StaffList = await _context.ClinicalStaffs
                    .Where(s => s.Position == "Psychiatrist")
                    .Select(s => new SelectListItem
                    {
                        Value = s.ClinicalStaffID.ToString(),
                        Text = s.Firstname + " " + s.Lastname
                    })
                    .ToListAsync();

                // Get available time slots for the selected date
                if (model.SelectedDate != default(DateTime) && model.ClinicalStaffID > 0)
                {
                    var date = model.SelectedDate.Date;
                    var dow = date.DayOfWeek;

                    var slots = await _context.Availabilities
                        .Where(a => a.ClinicalStaffID == model.ClinicalStaffID
                                  && (a.SlotDate == date || (a.SlotDate == null && a.Day == dow))
                                  && a.Status == AvailabilityStatus.Available.ToString())
                        .OrderBy(a => a.StartTime)
                        .ToListAsync();

                    ViewBag.AvailableTimes = slots;
                    ViewBag.TimeSlotList = slots.Select(s => new SelectListItem
                    {
                        Value = s.StartTime.ToString(),
                        Text = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}",
                        Selected = s.StartTime.ToString() == model.TimeSlot
                    }).ToList();
                }
                else
                {
                    ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
                    ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
                }

                ViewBag.SelectedDate = model.SelectedDate;
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return View(model);
            }

            // Find the exact availability slot that was booked
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.ClinicalStaffID == model.ClinicalStaffID
                            && a.StartTime == TimeSpan.Parse(model.TimeSlot)
                            && (a.SlotDate == model.SelectedDate || (a.SlotDate == null && a.Day == model.SelectedDate.DayOfWeek)));

            if (availability != null)
            {
                // Mark the availability slot as scheduled
                availability.Status = AvailabilityStatus.Scheduled.ToString();
                _context.Availabilities.Update(availability);
            }

            // Set required fields
            model.Status = SchedulingStatus.Scheduled.ToString();

            // Get ClinicalStaffName
            var doctor = await _context.ClinicalStaffs
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == model.ClinicalStaffID);
            if (doctor != null)
            {
                model.ClinicalStaffName = $"{doctor.Firstname} {doctor.Lastname}";
            }

            // Find existing psychiatric assessment appointment for this patient
            var existingAppointment = await _context.NewAppointments
                .FirstOrDefaultAsync(a => a.PatientId == model.PatientId && a.Type == "Psychiatric Assessment");

            if (existingAppointment != null)
            {
                // Update existing appointment
                existingAppointment.ClinicalStaffID = model.ClinicalStaffID;
                existingAppointment.ScheduleTime = model.TimeSlot;
                existingAppointment.ScheduleDate = model.SelectedDate;
                existingAppointment.Type = model.VisitType;
                existingAppointment.Status = SchedulingStatus.Scheduled.ToString();
                existingAppointment.Notes = model.Description;
            }
            else
            {
                // If no existing appointment, create new one
                existingAppointment = new NewAppointment
                {
                    ClinicalStaffID = model.ClinicalStaffID,
                    PatientId = model.PatientId,
                    ScheduleTime = model.TimeSlot,
                    ScheduleDate = model.SelectedDate,
                    Type = model.VisitType,
                    Status = SchedulingStatus.Scheduled.ToString(),
                    Notes = model.Description
                };
                _context.NewAppointments.Add(existingAppointment);
            }

            // Assign psychiatrist to patient if not already assigned
            var doctorAssignment = await _context.ClinicalStaffPatients
                .FirstOrDefaultAsync(csp => csp.PatientId == model.PatientId);

            if (doctorAssignment != null)
            {
                if (doctorAssignment.ClinicalStaffId != model.ClinicalStaffID)
                {
                    // Remove old assignment (since PK cannot be changed)
                    _context.ClinicalStaffPatients.Remove(doctorAssignment);
                    await _context.SaveChangesAsync();

                    // Add new assignment
                    var newAssignment = new ClinicalStaffPatient
                    {
                        ClinicalStaffId = model.ClinicalStaffID ?? 0,
                        PatientId = model.PatientId
                    };
                    _context.ClinicalStaffPatients.Add(newAssignment);
                }
            }
            else
            {
                var newAssignment = new ClinicalStaffPatient
                {
                    ClinicalStaffId = model.ClinicalStaffID ?? 0,
                    PatientId = model.PatientId
                };
                _context.ClinicalStaffPatients.Add(newAssignment);
            }

            try
            {
                await _context.SaveChangesAsync();

                // --- added activity log + notification ---
                var pat = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == model.PatientId);
                var doc = await _context.ClinicalStaffs.FirstOrDefaultAsync(c => c.ClinicalStaffID == model.ClinicalStaffID);
                var appt = await _context.NewAppointments
                    .OrderByDescending(a => a.ScheduleId)
                    .FirstOrDefaultAsync(a => a.PatientId == model.PatientId && a.Type == model.VisitType);

                var user = User?.Identity?.Name ?? "System";
                if (appt != null)
                {
                    await _activityService.LogAsync(
                        user,
                        "Scheduled appointment",
                        $"{appt.Type} for {FullName(pat)} with {StaffName(doc)} on {appt.ScheduleDate:yyyy-MM-dd} {(appt.ScheduleTime ?? "(All Day)")}",
                        "Appointment",
                        "Info",
                        appt.PatientId);

                    await _activityService.NotifyAsync(
                        user,
                        $"Scheduled {appt.Type} for {FullName(pat)}",
                        type: "Success");
                }
                // --- end added ---
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                ModelState.AddModelError("", "Failed to save appointment: " + ex.Message);
                TempData["ErrorMessage"] = "Failed to save appointment.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Psychiatric appointment scheduled successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SelectPsyDoctor(ScheduleAppointmentViewModel model)
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

                return View("ScheduleAppointment", model);
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
                    model.PatientName = $"{patient.Firstname} {patient.Lastname}";
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

            return View("PsySchedule", model);
        }
        
        // POST: user picked a date (after picking a doctor)
        [HttpPost]
        public async Task<IActionResult> PsyGetDateAndTimeSlots(ScheduleAppointmentViewModel model)
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

                return View("ScheduleAppointment", model);
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
                            && appt.ScheduleDate == date
                            && (appt.Status == Enum.AppointmentEnum.Pending.ToString()
                                || appt.Status == Enum.AppointmentEnum.Confirmed.ToString()
                                || appt.Status == Enum.AppointmentEnum.Booked.ToString()))
                .ToListAsync();

            // 3. Filter out slots that overlap
            var freeSlots = slots
                .Where(slot =>
                    !takenAppointments.Any(appt =>
                        TimeSpan.Parse(appt.ScheduleTime) >= slot.StartTime &&
                        TimeSpan.Parse(appt.ScheduleTime) < slot.EndTime   // falls inside this availability
                    )
                )
                .ToList();

            foreach (var s in freeSlots)
            {
                Console.WriteLine(freeSlots);
            }

            //  Preserve patient fullname
            if (model.PatientId > 0)
            {
                var patient = await _context.Patients
                    .Where(p => p.PatientId == model.PatientId)
                    .Select(p => new { p.Firstname, p.Lastname })
                    .FirstOrDefaultAsync();

                if (patient != null)
                    model.PatientName = $"{patient.Firstname} {patient.Lastname}";
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

            return View("PsySchedule", model);
        }

       [HttpGet]
        public async Task<IActionResult> GeneralScheduling()
        {
            var vm = new ScheduleAppointmentViewModel();

            // Show all doctors by default
            ViewBag.StaffList = await _context.ClinicalStaffs
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                })
                .ToListAsync();

            // Get patients with status Intreatment or Admitted
            ViewBag.PatientList = await _context.Patients
                .Where(p => p.PatientStatus == PatientStatusEnum.InTreatment.ToString()
                        || p.PatientStatus == PatientStatusEnum.Admitted.ToString())
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.Firstname + " " + p.Lastname
                })
                .ToListAsync();

            ViewBag.PatientId = null;
            ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
            ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
            ViewBag.SelectedDate = null;

            return View("GeneralScheduling", vm);
        }

        // POST: Select doctor for general scheduling
        [HttpPost]
        public async Task<IActionResult> GeneralSelectDoctor(ScheduleAppointmentViewModel model)
        {
            if (model.ClinicalStaffID <= 0)
            {
                ViewBag.StaffList = await _context.ClinicalStaffs
                    .Select(s => new SelectListItem
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

                return View("GeneralScheduling", model);
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
                    model.PatientName = $"{patient.Firstname} {patient.Lastname}";
            }

            // Fill doctor list again
            ViewBag.StaffList = await _context.ClinicalStaffs
                .Select(s => new SelectListItem
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

            return View("GeneralScheduling", model);
        }

        // POST: Get date and time slots for general scheduling
        [HttpPost]
        public async Task<IActionResult> GeneralGetDateAndTimeSlots(ScheduleAppointmentViewModel model)
        {
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

                ViewBag.PatientList = await _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = p.Firstname + " " + p.Lastname,
                        Selected = model.PatientId > 0 && p.PatientId == model.PatientId
                    })
                    .ToListAsync();

                return View("GeneralScheduling", model);
            }

            var date = model.SelectedDate.Date;
            var dow = date.DayOfWeek;

            var slots = await _context.Availabilities
                .Where(a => a.ClinicalStaffID == model.ClinicalStaffID
                            && (
                                a.SlotDate == date
                                || (a.SlotDate == null && a.Day == dow)
                            )
                            && a.Status == Enum.AvailabilityStatus.Available.ToString())
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            var takenAppointments = await _context.NewAppointments
                .Where(appt => appt.ClinicalStaffID == model.ClinicalStaffID
                            && appt.ScheduleDate == date
                            && (appt.Status == Enum.AppointmentEnum.Pending.ToString()
                                || appt.Status == Enum.AppointmentEnum.Confirmed.ToString()
                                || appt.Status == Enum.AppointmentEnum.Booked.ToString()))
                .ToListAsync();

            var freeSlots = slots
                .Where(slot =>
                    !takenAppointments.Any(appt =>
                        TimeSpan.Parse(appt.ScheduleTime) >= slot.StartTime &&
                        TimeSpan.Parse(appt.ScheduleTime) < slot.EndTime
                    )
                )
                .ToList();

            if (model.PatientId > 0)
            {
                var patient = await _context.Patients
                    .Where(p => p.PatientId == model.PatientId)
                    .Select(p => new { p.Firstname, p.Lastname })
                    .FirstOrDefaultAsync();

                if (patient != null)
                    model.PatientName = $"{patient.Firstname} {patient.Lastname}";
            }

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

            ViewBag.TimeSlotList = freeSlots.Select(s => new SelectListItem
            {
                Value = s.StartTime.ToString(),
                Text = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}"
            }).ToList();

            return View("GeneralScheduling", model);
        }

        // POST: General scheduling
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneralScheduling(ScheduleAppointmentViewModel model)
        {
            // Always populate PatientName if PatientId is present
            if (model.PatientId > 0 && string.IsNullOrWhiteSpace(model.PatientName))
            {
                var patient = await _context.Patients
                    .Where(p => p.PatientId == model.PatientId)
                    .Select(p => new { p.Firstname, p.Lastname })
                    .FirstOrDefaultAsync();

                if (patient != null)
                    model.PatientName = $"{patient.Firstname} {patient.Lastname}";
            }

            // Validate required fields
            if (model.ClinicalStaffID <= 0)
            {
                ModelState.AddModelError("ClinicalStaffID", "Please select a doctor");
            }
            if (string.IsNullOrEmpty(model.TimeSlot))
            {
                ModelState.AddModelError("TimeSlot", "Please select a time slot");
            }
            if (model.SelectedDate == default(DateTime))
            {
                ModelState.AddModelError("SelectedDate", "Please select a date");
            }
            if (model.PatientId <= 0)
            {
                ModelState.AddModelError("PatientId", "Please select a patient");
            }
            if (string.IsNullOrEmpty(model.VisitType))
            {
                ModelState.AddModelError("VisitType", "Please select a visit type");
            }
            if (string.IsNullOrWhiteSpace(model.PatientName))
            {
                ModelState.AddModelError("PatientName", "Patient name could not be determined.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns
                ViewBag.StaffList = await _context.ClinicalStaffs
                    .Select(s => new SelectListItem
                    {
                        Value = s.ClinicalStaffID.ToString(),
                        Text = s.Firstname + " " + s.Lastname
                    })
                    .ToListAsync();

                ViewBag.PatientList = await _context.Patients
                    .Where(p => p.PatientStatus == PatientStatusEnum.InTreatment.ToString()
                             || p.PatientStatus == PatientStatusEnum.Admitted.ToString())
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = p.Firstname + " " + p.Lastname,
                        Selected = model.PatientId > 0 && p.PatientId == model.PatientId
                    })
                    .ToListAsync();

                // Get available time slots for the selected date
                if (model.SelectedDate != default(DateTime) && model.ClinicalStaffID > 0)
                {
                    var date = model.SelectedDate.Date;
                    var dow = date.DayOfWeek;

                    var slots = await _context.Availabilities
                        .Where(a => a.ClinicalStaffID == model.ClinicalStaffID
                                  && (a.SlotDate == date || (a.SlotDate == null && a.Day == dow))
                                  && a.Status == AvailabilityStatus.Available.ToString())
                        .OrderBy(a => a.StartTime)
                        .ToListAsync();

                    ViewBag.AvailableTimes = slots;
                    ViewBag.TimeSlotList = slots.Select(s => new SelectListItem
                    {
                        Value = s.StartTime.ToString(),
                        Text = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}",
                        Selected = s.StartTime.ToString() == model.TimeSlot
                    }).ToList();
                }
                else
                {
                    ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
                    ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
                }

                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                    }
                }


                ViewBag.SelectedDate = model.SelectedDate;
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                TempData["SuccessMessage"] = "Please fill in all required fields.";
                return View("GeneralScheduling", model);


            }

            // Find the exact availability slot that was booked
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.ClinicalStaffID == model.ClinicalStaffID
                            && a.StartTime == TimeSpan.Parse(model.TimeSlot)
                            && (a.SlotDate == model.SelectedDate || (a.SlotDate == null && a.Day == model.SelectedDate.DayOfWeek)));

            if (availability != null)
            {
                // Mark the availability slot as scheduled
                availability.Status = AvailabilityStatus.Scheduled.ToString();
                _context.Availabilities.Update(availability);
            }

            // Set required fields
            model.Status = SchedulingStatus.Scheduled.ToString();

            // Get ClinicalStaffName
            var doctor = await _context.ClinicalStaffs
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == model.ClinicalStaffID);
            if (doctor != null)
            {
                model.ClinicalStaffName = $"{doctor.Firstname} {doctor.Lastname}";
            }

            // Find existing appointment for this patient and type
            var existingAppointment = await _context.NewAppointments
                .FirstOrDefaultAsync(a => a.PatientId == model.PatientId && a.Type == model.VisitType);

            if (existingAppointment != null)
            {
                // Update existing appointment
                existingAppointment.ClinicalStaffID = model.ClinicalStaffID;
                existingAppointment.ScheduleTime = model.TimeSlot;
                existingAppointment.ScheduleDate = model.SelectedDate;
                existingAppointment.Type = model.VisitType;
                existingAppointment.Status = SchedulingStatus.Scheduled.ToString();
                existingAppointment.Notes = model.Description;
            }
            else
            {
                // If no existing appointment, create new one
                existingAppointment = new NewAppointment
                {
                    ClinicalStaffID = model.ClinicalStaffID,
                    PatientId = model.PatientId,
                    ScheduleTime = model.TimeSlot,
                    ScheduleDate = model.SelectedDate,
                    Type = model.VisitType,
                    Status = SchedulingStatus.Scheduled.ToString(),
                    Notes = model.Description
                };
                _context.NewAppointments.Add(existingAppointment);
            }

            // Assign doctor to patient if not already assigned
            var doctorAssignment = await _context.ClinicalStaffPatients
                .FirstOrDefaultAsync(csp => csp.PatientId == model.PatientId);

            if (doctorAssignment != null)
            {
                if (doctorAssignment.ClinicalStaffId != model.ClinicalStaffID)
                {
                    doctorAssignment.ClinicalStaffId = model.ClinicalStaffID ?? 0;
                    _context.ClinicalStaffPatients.Update(doctorAssignment);
                }
            }
            else
            {
                var newAssignment = new ClinicalStaffPatient
                {
                    ClinicalStaffId = model.ClinicalStaffID ?? 0,
                    PatientId = model.PatientId
                };
                _context.ClinicalStaffPatients.Add(newAssignment);
            }

            try
            {
                await _context.SaveChangesAsync();

                // --- added activity log + notification ---
                var pat = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == model.PatientId);
                var doc = await _context.ClinicalStaffs.FirstOrDefaultAsync(c => c.ClinicalStaffID == model.ClinicalStaffID);
                var appt = await _context.NewAppointments
                    .OrderByDescending(a => a.ScheduleId)
                    .FirstOrDefaultAsync(a => a.PatientId == model.PatientId && a.Type == model.VisitType);

                var user = User?.Identity?.Name ?? "System";
                if (appt != null)
                {
                    await _activityService.LogAsync(
                        user,
                        "Scheduled appointment",
                        $"{appt.Type} for {FullName(pat)} with {StaffName(doc)} on {appt.ScheduleDate:yyyy-MM-dd} {(appt.ScheduleTime ?? "(All Day)")}",
                        "Appointment",
                        "Info",
                        appt.PatientId);

                    await _activityService.NotifyAsync(
                        user,
                        $"Scheduled {appt.Type} for {FullName(pat)}",
                        type: "Success");
                }
                // --- end added ---
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                ModelState.AddModelError("", "Failed to save appointment: " + ex.Message);
                TempData["ErrorMessage"] = "Failed to save appointment.";
                return View("GeneralScheduling", model);
            }

            TempData["SuccessMessage"] = "Appointment scheduled successfully.";
            return RedirectToAction("Index");
        }
    }
}

