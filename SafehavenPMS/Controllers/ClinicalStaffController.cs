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
        public async Task<IActionResult> Profile(int id)
        {
            // Query staff with address and assigned patients
            var staff = await _context.ClinicalStaffs
                .Include(csp => csp.ClinicalStaffPatients)
                    .ThenInclude(pa => pa.Patient)
                .FirstOrDefaultAsync(s => s.ClinicalStaffID == id);

            // Null check BEFORE using the object
            if (staff == null)
            {
                TempData["Error"] = "Staff not found";
                return RedirectToAction("Index"); // or wherever appropriate
            }

            // Map availabilities from DB into ViewModel list
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
                    Days = a.Days.Select(d => new DayAvailabilityViewModel
                    {
                        DayId = d.DayId,
                        DayName = d.DayName,
                        IsAvailable = d.IsAvailable,
                        TimeSlots = d.TimeSlots.Select(ts => new TimeSlotViewModel
                        {
                            TimeSlotId = ts.TimeSlotId,
                            StartTime = ts.StartTime,
                            EndTime = ts.EndTime
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            // Build the view model
            var viewModel = new ClinicalStaffProfileViewModel
            {
                Staffs = new List<ClinicalStaff> { staff },
                Patients = staff.ClinicalStaffPatients
                                .Select(csp => csp.Patient)
                                .Distinct()
                                .ToList(),
                Availability = availabilities // pass the populated list here
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
    }
}
