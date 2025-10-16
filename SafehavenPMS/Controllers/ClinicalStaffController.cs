using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class ClinicalStaffController : Controller
    {
        //Inject Context or services if needed
        private readonly SafehavenPMSContext _context;

        //Call out object for Static data seeding

        //Call out object for UploadPhotoServices if needed
        private readonly UploadPhotoServices _uploadPhotoServices;

        //Call out object for uploading image to cloudinary
        private readonly CloudinaryServices _cloudinaryServices;

        //Constructor to initialize the context
        public ClinicalStaffController(SafehavenPMSContext context)
        {
            _context = context;
            _uploadPhotoServices = new UploadPhotoServices();

            //Set up cloudinary config
            _cloudinaryServices = new CloudinaryServices(
               new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build()
           );
        }
        public async Task<IActionResult> Index(int? page = 1, int? pageSize = 10, string searchQuery = null, string status = null, string sortOrder = null, string sortBy = null)
        {
            var query = _context.ClinicalStaffs
                .Where(d => d.IsDeleted == false)
                .AsQueryable();

            // total (non-deleted)
            int totalStaffCount = await _context.ClinicalStaffs.Where(d => d.IsDeleted == false).CountAsync();
            ViewBag.TotalStaffCount = totalStaffCount;

            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status ?? "";             // <- ensure this is set
            ViewBag.SortBy = sortBy ?? "";
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // Apply search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(s =>
                    s.Firstname.ToLower().Contains(searchQuery) ||
                    s.Lastname.ToLower().Contains(searchQuery) ||
                    s.Position.ToLower().Contains(searchQuery));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(s => s.IsActive);
                else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(s => !s.IsActive);
            }


            // Apply sorting
            if (string.IsNullOrEmpty(sortOrder))
            {
                // Default: newest to oldest by CreatedAt
                query = query.OrderByDescending(s => s.CreatedAt);
            }
            else
            {
                // Toggle by Firstname
                query = sortOrder == "ascending"
                    ? query.OrderBy(s => s.Firstname).ThenBy(s => s.Lastname)
                    : query.OrderByDescending(s => s.Firstname).ThenBy(s => s.Lastname);
            }

            // Calculate total pages for filtered results
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            // Ensure page is within valid range
            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            // Apply pagination
            var staffList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            return View(staffList);
        }
        [HttpGet]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Index", new { searchQuery, page = 1, pageSize = ViewBag.PageSize ?? 10, status = ViewBag.Status, sortOrder = ViewBag.SortOrder });
        }

        [HttpGet]
        public IActionResult FilterStatus(string status)
        {
            return RedirectToAction("Index", new { status, page = 1, pageSize = ViewBag.PageSize ?? 10, searchQuery = ViewBag.SearchQuery, sortOrder = ViewBag.SortOrder });
        }

        [HttpGet]
        public IActionResult SortBy(string sortOrder)
        {
            return RedirectToAction("Index", new { sortOrder, page = 1, pageSize = ViewBag.PageSize ?? 10, searchQuery = ViewBag.SearchQuery, status = ViewBag.Status });
        }
        [HttpGet]
        public async Task<IActionResult> Profile(int id, DateTime? startDateInput = null, DateTime? endDateInput = null)
        {
            // 1. Get staff
            var staff = await _context.ClinicalStaffs
                .Include(csp => csp.ClinicalStaffPatients)
                    .ThenInclude(pa => pa.Patient)
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == id);

            if (staff == null)
            {
                TempData["Error"] = "Staff not found";
                return RedirectToAction("Index");
            }

            // 2. Get availabilities (template)
            var availabilities = await _context.Availabilities
                                     .Where(a => a.ClinicalStaffID == id)
                                     .ToListAsync();

            // 3. Date range
            var startDate = startDateInput ?? DateTime.Today;
            var endDate = endDateInput ?? startDate.AddDays(6);

            // 4. Get appointments for this doctor within the range
            var appointments = await _context.NewAppointments
                .Where(appt => appt.ClinicalStaffID == id
                               && appt.ScheduleDate >= startDate
                               && appt.ScheduleDate <= endDate
                               && appt.Status == "Booked")
                .ToListAsync();

            // 5. Override availability with Booked info
            foreach (var slot in availabilities)
            {
                var bookedMatch = appointments.FirstOrDefault(appt =>
                    // Case 1: Specific slot with date
                    (slot.SlotDate.HasValue && appt.ScheduleDate?.Date == slot.SlotDate.Value.Date
                                              && appt.ScheduleTime == slot.StartTime.ToString(@"hh\:mm"))
                    ||
                    // Case 2: Recurring slot (DayOfWeek match)
                    (!slot.SlotDate.HasValue && appt.ScheduleDate?.DayOfWeek == slot.Day
                                              && appt.ScheduleTime == slot.StartTime.ToString(@"hh\:mm"))
                );

                if (bookedMatch != null)
                {
                    slot.Status = "Unavailable";   // mark slot unavailable in UI
                    slot.Notes = "Booked";         // extra info
                }
            }

            // 6. Build VM
            var model = System.Enum.GetValues(typeof(DayOfWeek))
                         .Cast<DayOfWeek>()
                         .Select(d => new DayAvailabilityViewModel
                         {
                             Day = d,
                             Slots = new List<AvailabilityViewModel>
                             {
                        new AvailabilityViewModel(),
                        new AvailabilityViewModel()
                             }
                         }).ToList();

            var viewModel = new ClinicalStaffProfileViewModel
            {
                Staffs = new List<ClinicalStaff> { staff },
                Patients = staff.ClinicalStaffPatients
                                .Select(csp => csp.Patient)
                                .Distinct()
                                .ToList(),
                Availability = availabilities,
                Days = model
            };

            ViewBag.ClinicalStaffId = id;
            return View(viewModel);
        }


        //Action to add a new clinical staff member 
        [HttpGet]
        public IActionResult AddNewClinicalStaff()
        {
            //return staff that position == Physician
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewClinicalStaff(AddClinicalStaffViewModel model)
        {
            // Remove ImageURL from validation since we set it manually later
            ModelState.Remove("Filename");

            // Check validation
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;

                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            // Locally upload the photo before saving
            string tempUrl = string.Empty;
            string filename = null;

            if (model.ImageProfile != null && model.ImageProfile.Length > 0)
            {
                filename = _uploadPhotoServices.UploadPhoto(model.ImageProfile);

                if (!string.IsNullOrEmpty(filename))
                {
                    string localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filename);
                    if (!System.IO.File.Exists(localPath))
                    {
                        TempData["Error"] = "Profile image file not found. Please upload a valid image.";
                        return View(model);
                    }

                    try
                    {
                        using var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                        tempUrl = _cloudinaryServices.UploadImageAsync(fileStream, Path.GetFileName(localPath)).Result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        TempData["Error"] = "Failed to upload profile image: " + ex.Message;
                        _uploadPhotoServices.DeletePhoto(filename);
                        return View(model);
                    }

                    _uploadPhotoServices.DeletePhoto(filename); // Clean up temp file
                }
            }

            // Get the latest ClinicalStaffID and generate next reference number
            var lastStaff = await _context.ClinicalStaffs
                .OrderByDescending(c => c.ClinicalStaffID)
                .FirstOrDefaultAsync();

            int nextId = (lastStaff != null ? lastStaff.ClinicalStaffID + 1 : 1);
            string refId = $"CS-{nextId.ToString("D7")}"; // Pads to 7 digits: CS-0000001

            var staff = new ClinicalStaff
            {
                Firstname = model.Firstname,
                MiddleName = model.MiddleName,
                Lastname = model.Lastname,
                Sex = model.Sex,
                PhoneNumber = model.PhoneNumber,
                Position = model.Position,
                ProfilePictureURL = tempUrl,
                PRC_Licensed = model.RPC_Licensed,
                Email = model.Email,
                CreatedAt = DateTime.Now,
                ClinicalStaffRefId = refId,
                IsActive = true,
                Address = $"{model.House_Unit}, {model.Street}, {model.Subdivision_Village}, {model.Barangay}, {model.City}, {model.Province}"
            };


            try
            {
                await _context.ClinicalStaffs.AddAsync(staff);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            // Clean up the uploaded file reference in session
            model.ImageProfile = null;


            TempData["SuccessMessage"] = "Clinical staff added successfully!";
            return RedirectToAction("Confirmation", "Account", new { id = staff.ClinicalStaffID } );
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.ClinicalStaffs
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == id);

            if (staff == null)
            {
                return NotFound();
            }

            // Parse address if it exists
            string houseUnit = "", street = "", subdivision = "", barangay = "", city = "", province = "";
            if (!string.IsNullOrEmpty(staff.Address))
            {
                var addressParts = staff.Address.Split(',');
                if (addressParts.Length >= 6)
                {
                    houseUnit = addressParts[0].Trim();
                    street = addressParts[1].Trim();
                    subdivision = addressParts[2].Trim();
                    barangay = addressParts[3].Trim();
                    city = addressParts[4].Trim();
                    province = addressParts[5].Trim();
                }
            }

            var viewModel = new AddClinicalStaffViewModel
            {
                ClinicalStaffID = staff.ClinicalStaffID,
                Firstname = staff.Firstname,
                MiddleName = staff.MiddleName,
                Lastname = staff.Lastname,
                Sex = staff.Sex,
                PhoneNumber = staff.PhoneNumber,
                Position = staff.Position,
                RPC_Licensed = staff.PRC_Licensed,
                Email = staff.Email,
                House_Unit = houseUnit,
                Street = street,
                Subdivision_Village = subdivision,
                Barangay = barangay,
                City = city,
                Province = province,
                // ImageProfile is IFormFile and not set here (handled on POST)
                Filename = staff.ProfilePictureURL, // Map existing image URL if needed
            };
            Console.WriteLine(viewModel.Filename);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, AddClinicalStaffViewModel staff)
        {
            Console.WriteLine($"Received ID: {id}, Staff ClinicalStaffID: {staff.ClinicalStaffID}");
            if (id != staff.ClinicalStaffID)
            {
                return BadRequest("ID mismatch between route and form data.");
            }

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                        TempData["ValidationErrors"] = TempData["ValidationErrors"]?.ToString() + $"<br/>Field: {key} - Error: {error.ErrorMessage}";
                    }
                }
                return View(staff);
            }

            var existingStaff = await _context.ClinicalStaffs
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == id);

            if (existingStaff == null)
            {
                return NotFound();
            }

            // Update basic staff properties
            existingStaff.Firstname = staff.Firstname;
            existingStaff.MiddleName = staff.MiddleName;
            existingStaff.Lastname = staff.Lastname;
            existingStaff.Sex = staff.Sex;
            existingStaff.PhoneNumber = staff.PhoneNumber;
            existingStaff.Position = staff.Position;
            existingStaff.PRC_Licensed = staff.RPC_Licensed;
            existingStaff.Email = staff.Email;

            // Update address
            existingStaff.Address = $"{staff.House_Unit}, {staff.Street}, {staff.Subdivision_Village}, {staff.Barangay}, {staff.City}, {staff.Province}";

            // Handle profile picture update
            if (staff.ImageProfile != null && staff.ImageProfile.Length > 0)
            {
                string filename = _uploadPhotoServices.UploadPhoto(staff.ImageProfile);
                string localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filename.TrimStart('/'));

                if (System.IO.File.Exists(localPath))
                {
                    try
                    {
                        using var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

                        string photoUrl = await _cloudinaryServices.UploadImageAsync(fileStream, Path.GetFileName(localPath));
                        existingStaff.ProfilePictureURL = photoUrl;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error uploading image: " + ex.Message);
                        TempData["Error"] = "Failed to upload profile image: " + ex.Message;
                        return View(staff);
                    }
                    _uploadPhotoServices.DeletePhoto(filename);
                }
            }

            try
            {
                _context.ClinicalStaffs.Update(existingStaff);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Clinical staff updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicalStaffExists(id))
                {
                    TempData["Error"] = "Staff member not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "An error occurred while updating the staff member.";
                    return View(staff);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating staff: " + ex.Message);
                TempData["Error"] = "An error occurred while updating the staff member.";
                return View(staff);
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if ClinicalStaff exists
        private bool ClinicalStaffExists(int id)
        {
            return _context.ClinicalStaffs.Any(e => e.ClinicalStaffID == id);
        }


        // GET action to display delete confirmation page for a clinical staff member
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _context.ClinicalStaffs.FindAsync(id);
            if (staff == null)
            {
            TempData["Error"] = "Error to delete Staff";
            }
            return View(staff);
        }

        // POST action to handle soft delete of a clinical staff member
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.ClinicalStaffs.FindAsync(id);

            if (staff != null)
            {
            staff.IsDeleted = true;
            _context.ClinicalStaffs.Update(staff);
            await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST action to add or update availability slots for a clinical staff member
        [HttpPost]
        public async Task<IActionResult> AddAvailability(int ClinicalStaffID, List<string> days, int startTime, int endTime, string notes)
        {
            // Validate input
            if (days == null || !days.Any() || startTime > endTime)
            {
            TempData["Error"] = "Please select valid days and time range.";
            return RedirectToAction("Profile", new { id = ClinicalStaffID });
            }

            // For each selected day, merge new hours into existing day slots (avoid duplicates).
            foreach (var dayName in days)
            {
            if (!System.Enum.TryParse<DayOfWeek>(dayName, true, out var dayOfWeek))
                continue;

            // Load existing recurring slots for this staff + day (SlotDate == null means template/recurring)
            var existingDaySlots = await _context.Availabilities
                .Where(a => a.ClinicalStaffID == ClinicalStaffID && a.Day == dayOfWeek && a.SlotDate == null)
                .ToListAsync();

            for (int hour = startTime; hour <= endTime; hour++)
            {
                var existingSlot = existingDaySlots.FirstOrDefault(a => a.StartTime.Hours == hour);
                if (existingSlot != null)
                {
                // Update existing slot instead of creating duplicate.
                // Do not overwrite Scheduled slots � keep them scheduled.
                if (!string.Equals(existingSlot.Status, AvailabilityStatus.Scheduled.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    existingSlot.Status = AvailabilityStatus.Available.ToString();
                }

                // Update notes only when provided
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    existingSlot.Notes = notes;
                }

                _context.Availabilities.Update(existingSlot);
                }
                else
                {
                // Create new slot for the missing hour
                var slot = new Availability
                {
                    ClinicalStaffID = ClinicalStaffID,
                    Day = dayOfWeek,
                    StartTime = new TimeSpan(hour, 0, 0),
                    EndTime = new TimeSpan(hour + 1, 0, 0),
                    Status = AvailabilityStatus.Available.ToString(),
                    Notes = notes
                };
                _context.Availabilities.Add(slot);
                }
            }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Availability added/updated successfully!";
            return RedirectToAction("Profile", new { id = ClinicalStaffID });
        }
        
        // View model to represent a single availability time slot
        public class AvailabilitySlotViewModel
        {
            public string Day { get; set; }
            public int Hour { get; set; }
            public string Status { get; set; }
        }
        
        // View model to save multiple availability slots at once
        public class SaveAvailabilityViewModel
        {
            public int ClinicalStaffID { get; set; }
            public Dictionary<string, Dictionary<int, AvailabilitySlotViewModel>> Slots { get; set; }
        }
        
        // POST action to save multiple availability slots in one request
        [HttpPost]
        public IActionResult SaveAvailability(SaveAvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
            return RedirectToAction("Index", new { tab = "availability", error = "Invalid data submitted" });
            }
        
            try
            {
            var existingAvailability = _context.Availabilities
                .Where(a => a.ClinicalStaffID == model.ClinicalStaffID)
                .ToList();
        
            foreach (var daySlots in model.Slots)
            {
                foreach (var hourSlot in daySlots.Value)
                {
                var slot = hourSlot.Value;
                var normalizedDay = slot.Day.Substring(0, 1).ToUpper() + slot.Day.Substring(1).ToLower();
                var existing = existingAvailability.FirstOrDefault(a => 
                    a.Day.ToString() == normalizedDay && 
                    a.StartTime.Hours == slot.Hour);
        
                if (existing != null)
                {
                    existing.Status = slot.Status;
                }
                else
                {
                    _ = _context.Availabilities.Add(new Availability
                    {
                    ClinicalStaffID = model.ClinicalStaffID,
                    Day = System.Enum.Parse<DayOfWeek>(slot.Day.Substring(0, 1).ToUpper() + slot.Day.Substring(1).ToLower()),
                    StartTime = new TimeSpan(slot.Hour, 0, 0),
                    Status = slot.Status
                    });
                }
                }
            }
        
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Availability added/updated successfully!";
            return RedirectToAction("Profile", new { id = existingAvailability.FirstOrDefault()?.ClinicalStaffID });
            }
            catch (Exception ex)
            {
            Console.WriteLine("Error: " + ex.Message);
            // Log the exception here
            return RedirectToAction("Index", new { tab = "availability", error = "Failed to update availability" });
            }
        }
    }
}

