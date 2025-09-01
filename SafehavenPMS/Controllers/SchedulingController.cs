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
            var query = _context.Patients
                .Include(i => i.IntakeForm)
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .AsQueryable();

            // Get waitlisted count (patients with Waitlisted status)
            ViewBag.WaitlistedCount = await _context.Schedulings
                .CountAsync(p => p.Status == PatientStatusEnum.Pending.ToString());


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
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.PatientStatus == status);
            }

            // Apply sorting
            query = sortOrder == "ascending"
                ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                : query.OrderByDescending(p => p.CreatedAt);

            // Get total count for pagination
            ViewBag.TotalPatientCount = await query.CountAsync();

            // Apply pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            // Map to view model
            var schedulingVM = await _context.Schedulings.Select(p => new SchedulingViewModel
            {
                ScheduleId = p.ScheduleId,
                PatientId = p.PatientId,
                ClinicalStaffID = p.ClinicalStaffID,
                Type = p.Type,
                ScheduleDate = p.ScheduleDate,
                ScheduleTime = p.ScheduleTime,
                Status = p.Status,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                PatientName = $"{p.Patient.Firstname} {p.Patient.Lastname}",
                ClinicalStaffName = $"{p.ClinicalStaff.Firstname} {p.ClinicalStaff.Lastname}"
            }).ToListAsync();

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

        //Schedule initial assessment patient -> physician

        [HttpGet]
        public async Task<IActionResult> ScheduleAppointment(int id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            var viewModel = new ScheduleAppointmentViewModel
            {
                PatientId = patient.PatientId,
                PatientName = $"{patient.Firstname} {patient.Lastname}",
                SelectedDate = DateTime.Today
            };

            // Get available clinical staff (doctors)
            var doctors = await _context.ClinicalStaffs
                .Where(cs => cs.Position == "Physician")
                .Select(cs => new SelectListItem
                {
                    Value = cs.ClinicalStaffID.ToString(),
                    Text = $"{cs.Firstname} {cs.Lastname}"
                })
                .ToListAsync();

            ViewBag.Doctors = doctors;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDate(DateTime selectedDate, int patientId, int? clinicalStaffId)
        {
            // Get available time slots for the selected date and doctor
            var availableSlots = await GetAvailableTimeSlots(selectedDate, clinicalStaffId);
            ViewBag.AvailableTimes = availableSlots;

            // Get patient and doctor info for the form
            var patient = await _context.Patients.FindAsync(patientId);
            var doctor = clinicalStaffId.HasValue ?
                await _context.ClinicalStaffs.FindAsync(clinicalStaffId.Value) : null;

            var viewModel = new ScheduleAppointmentViewModel
            {
                PatientId = patientId,
                PatientName = $"{patient.Firstname} {patient.Lastname}",
                ClinicalStaffID = clinicalStaffId,
                ClinicalStaffName = doctor != null ? $"{doctor.Firstname} {doctor.Lastname}" : null,
                SelectedDate = selectedDate
            };

            return View("ScheduleAppointment", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ScheduleAppointment(ScheduleAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var scheduling = new Scheduling
                {
                    PatientId = model.PatientId,
                    ClinicalStaffID = model.ClinicalStaffID,
                    Type = model.VisitType,
                    ScheduleDate = model.SelectedDate,
                    ScheduleTime = model.TimeSlot,
                    Notes = model.Description,
                    Status = SchedulingStatus.Pending.ToString(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.Schedulings.Add(scheduling);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment scheduled successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error scheduling appointment. Please try again.");
                return View(model);
            }
        }

        private async Task<List<TimeSlot>> GetAvailableTimeSlots(DateTime date, int? doctorId)
        {
            // Define your time slots (example: 9 AM to 5 PM, 1-hour intervals)
            var allTimeSlots = new List<TimeSlot>();
            var startTime = new TimeSpan(9, 0, 0); // 9 AM
            var endTime = new TimeSpan(17, 0, 0);  // 5 PM
            var interval = TimeSpan.FromHours(1);

            // Get existing appointments for this date and doctor
            var existingAppointments = await _context.Schedulings
                .Where(s => s.ScheduleDate == date.Date &&
                       (!doctorId.HasValue || s.ClinicalStaffID == doctorId))
                .Select(s => s.ScheduleTime)
                .ToListAsync();

            // Generate available time slots
            for (var time = startTime; time < endTime; time += interval)
            {
                var timeString = time.ToString(@"hh\:mm");
                if (!existingAppointments.Contains(timeString))
                {
                    allTimeSlots.Add(new TimeSlot
                    {
                        StartTime = time,
                        EndTime = time.Add(interval)
                    });
                }
            }

            return allTimeSlots;
        }
    }

    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
