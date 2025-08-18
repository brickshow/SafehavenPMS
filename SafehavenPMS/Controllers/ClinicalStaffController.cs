using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System.Text.Json;

namespace SafehavenPMS.Controllers
{
    public class ClinicalStaffController : Controller
    {
        //Inject Context or services if needed
        private readonly SafehavenPMSContext _context;


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
        public async Task<IActionResult> Index(int? page = 1, int? pageSize = 10, string searchQuery = null, string status = null, string sortOrder = null)
        {
            var query = _context.ClinicalStaffs
                .Where(d => d.IsDeleted == false)
                .AsQueryable();

            // Calculate total staff count (unfiltered, unpaged)
            int totalStaffCount = await _context.ClinicalStaffs.CountAsync();
            ViewBag.TotalStaffCount = totalStaffCount;
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // Apply search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(s =>
                    s.Firstname.ToLower().Contains(searchQuery) ||
                    s.Lastname.ToLower().Contains(searchQuery) ||
                    s.Specialty.ToLower().Contains(searchQuery) ||
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

        //Action for Profile

        [HttpGet]
        public async Task<IActionResult> Profile(int id, DateTime? startDateInput = null, DateTime? endDateInput = null)
        {
            // Query staff with address and assigned patients
            var staff = await _context.ClinicalStaffs
                .Include(csp => csp.ClinicalStaffPatients)
                    .ThenInclude(pa => pa.Patient)
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == id);

            if (staff == null)
            {
                TempData["Error"] = "Staff not found";
                return RedirectToAction("Index");
            }


            // Map existing availabilities from DB
            var availabilities = await _context.Availabilities
                .Where(a => a.ClinicalStaffID == id)
                .Include(a => a.Days)
                    .ThenInclude(d => d.TimeSlots)
                .Select(a => new AvailabilityViewModel
                {
                    ClinicalStaffID = a.ClinicalStaffID,
                    Title = a.Title,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    NoEndDate = a.NoEndDate,
                    Days = a.Days
                        .Select(d => new DayAvailabilityViewModel
                        {
                            DayId = d.DayId,
                            DayName = d.DayName,
                            IsAvailable = d.IsAvailable && d.TimeSlots.Any(ts => ts.IsAvailable)
                                            && DateTime.Today <= a.StartDate.AddDays(System.Enum.Parse<DayOfWeek>(d.DayName) - a.StartDate.DayOfWeek),
                            TimeSlots = d.TimeSlots.Select(ts => new TimeSlotViewModel
                            {
                                TimeSlotId = ts.TimeSlotId,
                                StartTime = ts.StartTime,
                                IsAvailable = ts.IsAvailable,
                                EndTime = ts.EndTime
                            }).ToList()
                        }).ToList()
                })
                .ToListAsync();

            // Use inputted dates or default to today + 6 days
            var startDate = startDateInput ?? DateTime.Today;
            var endDate = endDateInput ?? startDate.AddDays(6);

            // Build date range for new availability
            var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                      .Select(offset => startDate.AddDays(offset))
                                      .ToList();

            // Build NewAvailability
            var newAvailability = new AvailabilityViewModel
            {
                ClinicalStaffID = id,
                StartDate = startDate,
                EndDate = endDate,
                Days = dateRange.Select(date => new DayAvailabilityViewModel
                {
                    DayName = date.DayOfWeek.ToString(),
                    IsAvailable = false, // default
                    TimeSlots = new List<TimeSlotViewModel>() // empty by default
                }).ToList()
            };

            // Build the view model
            var viewModel = new ClinicalStaffProfileViewModel
            {
                Staffs = new List<ClinicalStaff> { staff },
                Patients = staff.ClinicalStaffPatients
                                .Select(csp => csp.Patient)
                                .Distinct()
                                .ToList(),
                Availability = availabilities,
                NewAvailability = newAvailability
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
        public IActionResult AddNewClinicalStaff(AddClinicalStaffViewModel model)
        {
            // Remove ImageURL from validation since we set it manually later
            ModelState.Remove("Filename");
            // Check Validation
            if (!ModelState.IsValid)
            {
                // Log ModelState errors to console
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;

                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                    }
                }
                return View(model); // Pass model back to preserve entered data
            }

            // Check if Hire Date is in the future
            if (model.HireDate > DateTime.Now)
            {
                TempData["Error"] = "Hire Date must not be in the future.";
                return View(model);
            }

            //Locally upload the photo before saving to session
            model.Filename = _uploadPhotoServices.UploadPhoto(model.ImageProfile);

            //Upload the Photo to cloudinary
            string tempUrl = string.Empty; // Default image
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
                        _uploadPhotoServices.DeletePhoto(filename); // Clean up temp file on failure
                        return View(model);
                    }
                    _uploadPhotoServices.DeletePhoto(filename); // Clean up temp file on success
                }
            }

            //Save Clinical staff to database
            try
            {
                //Add the Clinincal Staff Information
                var staff = new ClinicalStaff
                {
                    Firstname = model.Firstname,
                    MiddleName = model.MiddleName,
                    Lastname = model.Lastname,
                    Sex = model.Sex,
                    PhoneNumber = model.PhoneNumber,
                    Specialty = model.Specialty,
                    Position = model.Position,
                    ProfilePictureURL = tempUrl,
                    PRC_Licensed = model.RPC_Licensed,
                    Email = model.Email,
                    HireDate = model.HireDate,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    Address = $"{model.House_Unit}, {model.Street}, {model.Subdivision_Village}, {model.Barangay}, {model.City}, {model.Province}"
                };

                //Save to database
                _context.ClinicalStaffs.Add(staff);
                _context.SaveChanges();

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error saving patient: " + ex.Message);
                TempData["Error"] = "There was an error saving the patient.";
                return View(); // Make sure there's a view for fallback here
            }


            //Remove the IFormFile in session
            model.ImageProfile = null;
            TempData["SuccessMessage"] = "Clinical staff added successfully!";

            return RedirectToAction("Index");
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

            var viewModel = new AddClinicalStaffViewModel
            {
                ClinicalStaffID = staff.ClinicalStaffID,
                Firstname = staff.Firstname,
                MiddleName = staff.MiddleName,
                Lastname = staff.Lastname,
                Sex = staff.Sex,
                PhoneNumber = staff.PhoneNumber,
                Specialty = staff.Specialty,
                Position = staff.Position,
                RPC_Licensed = staff.PRC_Licensed,
                Email = staff.Email,
                HireDate = staff.HireDate,
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
                .Include(s => s.Address)
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
            existingStaff.Specialty = staff.Specialty;
            existingStaff.Position = staff.Position;
            existingStaff.PRC_Licensed = staff.RPC_Licensed; // Fixed typo
            existingStaff.Email = staff.Email;
            existingStaff.HireDate = staff.HireDate;

            //// Handle address update
            //if (existingStaff.Address != null)
            //{
            //    existingStaff.Address.House_Unit = staff.House_Unit;
            //    existingStaff.Address.Street = staff.Street;
            //    existingStaff.Address.Subdivision_Village = staff.Subdivision_Village;
            //    existingStaff.Address.Barangay = staff.Barangay;
            //    existingStaff.Address.City = staff.City;
            //    existingStaff.Address.Province = staff.Province;
            //}
            //else if (!string.IsNullOrEmpty(staff.House_Unit) || !string.IsNullOrEmpty(staff.Street) ||
            //         !string.IsNullOrEmpty(staff.Subdivision_Village) || !string.IsNullOrEmpty(staff.Barangay) ||
            //         !string.IsNullOrEmpty(staff.City) || !string.IsNullOrEmpty(staff.Province))
            //{
            //    var newAddress = new Address
            //    {
            //        House_Unit = staff.House_Unit,
            //        Street = staff.Street,
            //        Subdivision_Village = staff.Subdivision_Village,
            //        Barangay = staff.Barangay,
            //        City = staff.City,
            //        Province = staff.Province
            //    };
            //    _context.Addresses.Add(newAddress);
            //    await _context.SaveChangesAsync();
            //    existingStaff.AddressID = newAddress.AddressID;
            //}

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
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicalStaffExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if ClinicalStaff exists
        private bool ClinicalStaffExists(int id)
        {
            return _context.ClinicalStaffs.Any(e => e.ClinicalStaffID == id);
        }


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

            TempData["SuccessMessage"] = $"Patient {staff.Firstname} {staff.Lastname} has been deleted successfully.";


            return RedirectToAction("Index");
        }

        //Action for adding new staff availability
        [HttpGet]
        public IActionResult SaveAvailability()
        {
            return PartialView("_Availability");
        }

        ////Showing all available days within inside rage date
        //[HttpGet]
        //public async Task<IActionResult> GetDates()
        //{

        //}
        [HttpPost]
        public IActionResult SubmitDate(AvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ModelState Errors ===");
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"Field: {kvp.Key} | Error: {error.ErrorMessage}");
                    }
                }

                // Optional: return view with model to display validation errors
                TempData["Error"] = "Some fields are invalid. Check console output.";
                return RedirectToAction("Profile", new { id = model.ClinicalStaffID });
            }

            // Print top-level availability info
            Console.WriteLine("=== Submitted Availability ===");
            Console.WriteLine($"ClinicalStaffID: {model.ClinicalStaffID}");
            Console.WriteLine($"Title: {model.Title}");
            Console.WriteLine($"StartDate: {model.StartDate:yyyy-MM-dd}");
            Console.WriteLine($"EndDate: {model.EndDate:yyyy-MM-dd}");
            Console.WriteLine($"NoEndDate: {model.NoEndDate}");

            // Loop through each day
            foreach (var day in model.Days)
            {
                Console.WriteLine($"\nDay: {day.DayName} (IsAvailable: {day.IsAvailable})");

                // Loop through each time slot
                foreach (var slot in day.TimeSlots)
                {
                    Console.WriteLine($"  TimeSlot: {slot.StartTime:hh\\:mm} - {slot.EndTime:hh\\:mm}");
                }
            }

            TempData["Info"] = "Availability data submitted. Check console output.";

            return RedirectToAction("Profile", new 
            { 
                id = model.ClinicalStaffID, 
                startDateInput = model.StartDate, 
                endDateInput = model.EndDate
            });
        }

        // Helper method to get all dates that match a given day name between start and end dates
        public List<DateTime> GetDatesForDayName(string dayName, DateTime startDate, DateTime? endDate)
        {
            var dates = new List<DateTime>(); // List to store matching dates

            // Convert string day name (e.g., "Monday") to DayOfWeek enum
            if (!System.Enum.TryParse<DayOfWeek>(dayName, true, out var dayOfWeek))
                return dates; // If invalid day name, return empty list

            var current = startDate; // Start iterating from StartDate
            var finalDate = endDate ?? startDate.AddYears(1); // Use EndDate, or default to 1 year later if null

            // Loop from start to end date
            while (current <= finalDate)
            {
                if (current.DayOfWeek == dayOfWeek) // Check if current date matches the requested day
                    dates.Add(current); // Add matching date to the list

                current = current.AddDays(1); // Move to next day
            }

            return dates; // Return all matching dates
        }

        [HttpPost]
        public IActionResult SaveAvailabilityJson([FromBody] AvailabilityViewModel model)
        {
            if (model == null)
                return Json(new { success = false }); // Return false if no data received

            // Create main Availability entity
            var availability = new Availability
            {
                ClinicalStaffID = model.ClinicalStaffID, // Associate with clinical staff
                Title = model.Title, // Availability title
                StartDate = model.StartDate, // Start of availability period
                EndDate = model.NoEndDate ? null : model.EndDate, // End date, or null if "no end date"
                NoEndDate = model.NoEndDate, // Flag for open-ended availability
                Days = new List<AvailabilityDay>() // Initialize list of days
            };

            // Loop through each day submitted in the model
            foreach (var dayVM in model.Days)
            {
                // Generate all actual dates for this day name within start-end range
                var dayDates = GetDatesForDayName(dayVM.DayName, model.StartDate, model.NoEndDate ? null : model.EndDate);

                // Loop through each date generated
                foreach (var date in dayDates)
                {
                    var dayEntity = new AvailabilityDay
                    {
                        DayName = dayVM.DayName, // Store the day name (e.g., "Monday")
                        IsAvailable = true, // Mark day as available
                        Date = date, // Save the actual date
                        TimeSlots = dayVM.TimeSlots.Select(ts => new TimeSlot
                        {
                            StartTime = TimeSpan.Parse(ts.StartTime.ToString(@"hh\:mm")), // Parse start time
                            EndTime = TimeSpan.Parse(ts.EndTime.ToString(@"hh\:mm")) // Parse end time
                        }).ToList() // Convert all time slots to list
                    };

                    availability.Days.Add(dayEntity); // Add day entity to Availability
                }
            }

            _context.Availabilities.Add(availability); // Add Availability to DB context
            _context.SaveChanges(); // Persist changes to database

            return Json(new { success = true }); // Return success response
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var day = await _context.AvailabilityDays
                            .Include(d => d.TimeSlots) // Include related timeslots
                                .Include(d => d.Availability)
                            .FirstOrDefaultAsync(d => d.DayId == id);

            if (day == null)
                return NotFound();

            try
            {
                // Remove all timeslots first
                _context.TimeSlots.RemoveRange(day.TimeSlots);

                // Then remove the day
                _context.AvailabilityDays.Remove(day);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            // Redirect to Profile with the staff id
            return RedirectToAction("Profile", new { id = day.Availability.ClinicalStaffID });
        }

        [HttpGet]
        public async Task<IActionResult> FilterByDate(int id, DateTime? selectedDate)
        {
            // Load the staff along with their patients
            var staff = await _context.ClinicalStaffs
                .Include(s => s.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.Patient)
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == id);

            if (staff == null)
            {
                TempData["Error"] = "Staff not found"; // Show error if staff does not exist
                return RedirectToAction("Index");
            }

            // Load availabilities with days and timeslots
            var availabilities = await _context.Availabilities
                .Where(a => a.ClinicalStaffID == id)
                .Include(a => a.Days)
                    .ThenInclude(d => d.TimeSlots)
                .ToListAsync();

            var filteredAvailabilities = availabilities
                .Select(a =>
                {
                    a.Days = a.Days
                        .Where(d =>
                        {
                            // Show all available days if no date selected
                            if (!selectedDate.HasValue)
                                return d.IsAvailable;

                            // Convert DayName to DayOfWeek
                            var dayOfWeek = System.Enum.Parse<DayOfWeek>(d.DayName.Trim(), ignoreCase: true);

                            // Match selected date
                            bool isMatchingDay = d.IsAvailable && selectedDate?.DayOfWeek == dayOfWeek;

                            // Ensure selected date is within availability range
                            bool withinRange = selectedDate?.Date >= a.StartDate.Date &&
                                               (a.NoEndDate || (a.EndDate.HasValue && selectedDate?.Date <= a.EndDate.Value.Date));

                            return isMatchingDay && withinRange;
                        })
                        .ToList();

                    return a;
                })
                .Where(a => a.Days.Any())
                .ToList();


            // Map filtered availabilities to view models
            var filteredAvailabilityViewModels = filteredAvailabilities
                .Select(a => new AvailabilityViewModel
                {
                    ClinicalStaffID = a.ClinicalStaffID,
                    Title = a.Title,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    NoEndDate = a.NoEndDate,
                    Days = a.Days.Select(d => new DayAvailabilityViewModel
                    {
                        DayId = d.DayId,
                        DayName = d.DayName,
                        IsAvailable = d.IsAvailable,
                        TimeSlots = d.TimeSlots.Select(ts => new TimeSlotViewModel
                        {
                            TimeSlotId = ts.TimeSlotId,
                            StartTime = ts.StartTime,
                            EndTime = ts.EndTime,
                            IsAvailable = ts.IsAvailable
                        }).ToList()
                    }).ToList()
                })
                .ToList();

            // Build the final view model
            var viewModel = new ClinicalStaffProfileViewModel
            {
                Staffs = new List<ClinicalStaff> { staff }, // Single staff
                Patients = staff.ClinicalStaffPatients.Select(csp => csp.Patient).Distinct().ToList(),
                Availability = filteredAvailabilityViewModels
            };

            ViewBag.ClinicalStaffId = id;
            ViewBag.SelectedDate = selectedDate;
            return View("Profile", viewModel); // Render the Profile view
        }

    }
}
