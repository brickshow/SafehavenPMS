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

        [HttpPost]
        public async Task<IActionResult> AddAvailabilityDate([FromBody] AvailabilityViewModel request)
        {
            try
            {
                // Validate the request
                if (string.IsNullOrEmpty(request.Title))
                {
                    return Json(new { success = false, message = "Title is required" });
                }

                if (request.StartDate < DateTime.Now)
                {
                    return Json(new { success = false, message = "The start date must be today or in the future" });
                }

                if (request.StartDate == default(DateTime))
                {
                    return Json(new { success = false, message = "Start date is required" });
                }

                if (!request.NoEndDate && request.EndDate == null)
                {
                    return Json(new { success = false, message = "End date is required unless 'No end date' is checked" });
                }

                if (!request.NoEndDate && request.EndDate <= request.StartDate)
                {
                    return Json(new { success = false, message = "End date must be after start date" });
                }

                if (request.Days == null || !request.Days.Any())
                {
                    return Json(new { success = false, message = "At least one day with time slots must be selected" });
                }

                // Create the main Availability record
                var availability = new Availability
                {
                    Title = request.Title,
                    StartDate = request.StartDate,
                    EndDate = request.NoEndDate ? null : request.EndDate,
                    ClinicalStaffID = request.ClinicalStaffID,
                    
                    Days = new List<AvailabilityDay>()
                };

                // Process each selected day
                foreach (var dayRequest in request.Days)
                {
                    if (dayRequest.TimeSlots == null || !dayRequest.TimeSlots.Any())
                        continue; // Skip days without time slots

                    var availabilityDay = new AvailabilityDay
                    {
                        DayName = dayRequest.DayName,
                        IsAvailable = true,
                        TimeSlots = new List<TimeSlot>()
                    };

                    // Process time slots for this day
                    foreach (var timeSlotRequest in dayRequest.TimeSlots)
                    {
                            if (timeSlotRequest.EndTime > timeSlotRequest.StartTime) // Validate that end time is after start time
                            {
                                var timeSlot = new TimeSlot
                                {
                                    StartTime = timeSlotRequest.StartTime,
                                    EndTime = timeSlotRequest.EndTime
                                };
                                availabilityDay.TimeSlots.Add(timeSlot);
                            }
                       
                    }

                    // Only add the day if it has valid time slots
                    if (availabilityDay.TimeSlots.Any())
                    {
                        availability.Days.Add(availabilityDay);
                    }
                }

                // Final validation - make sure we have at least one day with time slots
                if (!availability.Days.Any())
                {
                    return Json(new { success = false, message = "At least one day with valid time slots must be selected" });
                }

                if (!_context.ClinicalStaffs.Any(cs => cs.ClinicalStaffID == request.ClinicalStaffID))
                {
                    ModelState.AddModelError("", "Invalid Clinical Staff ID");
                    return View(request);
                }

                // Save to database
                _context.Availabilities.Add(availability); // Replace with your actual DbSet name
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Availability saved successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception (use your logging framework)
                // _logger.LogError(ex, "Error saving availability");

                return Json(new { success = false, message = "An error occurred while saving availability" });
            }
        }
        //Action for getting Patient name and staff name
        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            //Filter patient information
            var patient = await _context.Patients
                                .Where(p => p.PatientId == id)
                                .FirstOrDefaultAsync();

            //Check if patient is null
            if (patient == null)
                return Json(new List<string>());//Return empty if patient is null

            //Get the specific data to pass in view using json
            string patientName = $"{patient.Firstname} {patient.Lastname}";

            // Staff name (assuming only one staff per patient)
            var staff = patient.ClinicalStaffPatients
                .Select(csp => csp.ClinicalStaff)
                .FirstOrDefault();

            string staffName = staff != null
                ? $"{staff.Firstname} {staff.Lastname}"
                : "No staff assigned";

            // Return JSON object
            return Json(new
            {
                success = true,
                patientName,
                staffName
            });
        }

        //Action for getting availability time 
        [HttpGet]
        public async Task<IActionResult> GetAvailabilityTime(int id, DateTime date)
        {
            try
            {
                // Step 1: Retrieve the ClinicalStaffId for the given PatientId
                var patient = await _context.ClinicalStaffPatients
                                    .Include(p => p.ClinicalStaff)
                                    .FirstOrDefaultAsync(p => p.PatientId == id);

                Console.WriteLine($"Step 1: Retrieved staffId = {patient} for PatientId = {id}");

                if (patient == null) // Check for 0 since ClinicalStaffId is an int
                {
                    Console.WriteLine("Step 1: No clinical staff assigned to this patient");
                    return Json(new { success = false, message = "No clinical staff assigned to this patient" });
                }

                //Set staff ID based on patient query
                int staffId = patient.ClinicalStaffId;
                string patientname = $"{patient.Patient.Firstname} {patient.Patient.Lastname}";
                string staffName = $"{patient.ClinicalStaff.Firstname} {patient.ClinicalStaff.Lastname}";

                // Step 2: Get the day of the week for the provided date
                var dayOfWeek = date.ToString("dddd"); // e.g., "Monday"
                Console.WriteLine($"Step 2: dayOfWeek = {dayOfWeek} for date = {date}");

                // Step 3: Query availabilities for the staff, filtering by date and active days
                var availabilities = await _context.Availabilities
                                    .Include(a => a.Days)
                                        .ThenInclude(d => d.TimeSlots)
                                    .Where(a => a.ClinicalStaffID == staffId &&
                                            a.StartDate.Date <= date.Date &&
                                            (a.NoEndDate || a.EndDate == null || a.EndDate.Value.Date >= date.Date))
                                    .ToListAsync();

                Console.WriteLine($"Step 3: Found {availabilities.Count} availabilities for staffId = {staffId}");
                foreach (var a in availabilities)
                {
                    Console.WriteLine($"  AvailabilityId = {a.AvailabilityId}, StartDate = {a.StartDate}, EndDate = {a.EndDate}, NoEndDate = {a.NoEndDate}");
                    foreach (var d in a.Days)
                    {
                        Console.WriteLine($"    DayId = {d.DayId}, DayName = {d.DayName}, IsAvailable = {d.IsAvailable}");
                    }
                }

                var timeSlots = availabilities
                               .SelectMany(a => a.Days)
                               .Where(d => d.DayName == dayOfWeek && d.IsAvailable)
                               .SelectMany(d => d.TimeSlots)
                               .Select(ts => new {
                                   ts.TimeSlotId,
                                   //Convert Army time to AM/PM Format
                                   StartTime = DateTime.Today.Add(ts.StartTime).ToString("hh:mm tt"),
                                   EndTime = DateTime.Today.Add(ts.EndTime).ToString("hh:mm tt")
                               })
                               .Distinct()
                               .OrderBy(t => t.StartTime)
                               .ToList();

                Console.WriteLine($"Step 4: Found {timeSlots.Count} time slots for dayOfWeek = {dayOfWeek}");

                if (!timeSlots.Any())
                {
                    Console.WriteLine("Step 4: No time slots available due to filtering by dayOfWeek and IsAvailable");
                    return Json(new { success = false, message = "No time slots available for the specified date" });
                }

                return Json(new
                {
                    success = true,
                    timeSlots,
                    patientName = patientname, //Passing patient name to view for auto fill feature
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                // Log the exception (use your logging framework)
                // _logger.LogError(ex, "Error retrieving availability for patient {PatientId} on {Date}", id, date);
                return Json(new { success = false, message = "An error occurred while retrieving availability" });
            }
        }
    }
}
