using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System.Text.Json;

namespace SafehavenPMS.Controllers
{
    public class AppointmentController : Controller
    {
        //Inject Context or services if needed
        private readonly SafehavenPMSContext _context;

        //Constructor
        public AppointmentController(SafehavenPMSContext context)
        {
            _context = context;
        }
        public IActionResult AddAvailabilityDate(AvailabilityViewModel model)
        {
            // Check validation
            foreach (var entry in ModelState)
            {
                var key = entry.Key;
                var errors = entry.Value.Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                }
            }

            // Debug: print model as JSON
            string json = System.Text.Json.JsonSerializer.Serialize(model, new JsonSerializerOptions
            {
                WriteIndented = true // makes JSON pretty
            });
            Console.WriteLine(json);

            // Save in session
            HttpContext.Session.SetObject<AvailabilityViewModel>("AddAvailabilityDate", model);

            return RedirectToAction("Index", "ClinicalStaff"); // fixed parameter order
        }
        //[HttpPost]
        //public async Task<IActionResult> SaveAvailability([FromBody] AvailabilityViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (model.EndDate < model.StartDate)
        //    {
        //        return BadRequest(new { message = "End date must be after or equal to start date." });
        //    }

        //    try
        //    {
        //        foreach (var day in model.Days.Where(d => d.IsAvailable))
        //        {
        //            foreach (var slot in day.TimeSlots)
        //            {
        //                if (slot.StartTime == default || slot.EndTime == default)
        //                    continue;

        //                var availability = new SafehavenPMS.Models.Availability
        //                {
        //                    Title = model.Title,
        //                    StartDate = model.StartDate,
        //                    EndDate = model.EndDate ?? model.StartDate, // Fallback to StartDate if EndDate is null
        //                };

        //                _context.Availabilities.Add(availability);
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        return Ok(new { message = "Availability saved successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while saving availability", error = ex.Message });
        //    }
        //}
        public IActionResult Index()
        {
            return View();
        }
    }
}
