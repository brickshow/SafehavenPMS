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

namespace SafehavenPMS.Controllers
{
    public class SchedulingController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public SchedulingController(SafehavenPMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(
                   int? page = 1,
                   int? pageSize = 10,
                   string searchQuery = null,
                   string status = null,
                   string sortOrder = null)
        {
            // Query appointments with patient and staff information
            var query = _context.NewAppointments
                .Include(a => a.Patient)
                .Include(a => a.ClinicalStaff)
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
            query = sortOrder == "ascending"
                ? query.OrderBy(a => a.Patient.Firstname).ThenBy(a => a.Patient.Lastname)
                : query.OrderByDescending(a => a.CreatedAt);

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
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Index", new
            {
                searchQuery,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                status = ViewBag.Status,
                sortOrder = ViewBag.SortOrder
            });
        }

        public async Task<IActionResult> ScheduleAppointment(int? id)
        {
            var vm = new ScheduleAppointmentViewModel();
            var patient = await _context.Patients
                   .FirstOrDefaultAsync(p => p.PatientId == id.Value);

            // Get patient details if ID is provided
            if (id.HasValue && id > 0)
            {


                if (patient != null)
                {
                    vm.PatientId = patient.PatientId;
                    vm.PatientName = $"{patient.Firstname} {patient.Lastname}";
                }
            }

            ViewBag.StaffList = await _context.ClinicalStaffs
                .Where(s => s.Position == "Physician")
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                })
                .ToListAsync();

            // Only get patient if not already provided
            if (!id.HasValue)
            {
                ViewBag.PatientList = await _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientId.ToString(),
                        Text = p.Firstname + " " + p.Lastname,
                        Selected = id.HasValue && p.PatientId == id.Value
                    })
                    .ToListAsync();
            }

            ViewBag.PatientId = id;
            ViewBag.AvailableTimes = Enumerable.Empty<Availability>().ToList();
            ViewBag.TimeSlotList = Enumerable.Empty<SelectListItem>().ToList();
            ViewBag.SelectedDate = null;
            ViewBag.VisitType = patient?.NewAppointments?.FirstOrDefault()?.Type;

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
                .Select(s => new { s.ClinicalStaffID, FullName = (s.Firstname + " " + s.Lastname) })
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
                TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            }
            catch (Exception ex)
            {
                // optionally log ex
                TempData["Error"] = "Failed to cancel appointment.";
            }

            return RedirectToAction("Index");
        }
    }
}
