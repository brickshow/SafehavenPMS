using Microsoft.AspNetCore.Mvc;
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
    }
}
