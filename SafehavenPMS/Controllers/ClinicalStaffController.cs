using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Index()
        {
            // Default sort: newest to oldest
            ViewBag.SortOrder = "newest";
            //Populate data to table
            var staffList = await _context.ClinicalStaffs
                .Include(add => add.Address)
                .ToListAsync();

            //Return the list off staff
            return View(staffList);
        }

        // GET: ClinicalStaff/Search
        [HttpGet]
        public IActionResult Search(string searchQuery)
        {
            var staff = _context.ClinicalStaffs.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                staff = staff.Where(s =>
                    s.Firstname.ToLower().Contains(searchQuery) ||
                    s.Lastname.ToLower().Contains(searchQuery) ||
                    s.Specialty.ToLower().Contains(searchQuery) ||
                    s.Position.ToLower().Contains(searchQuery));
            }

            return View("Index", staff.ToList());
        }

        [HttpGet]
        public IActionResult FilterStatus(string status)
        {
            // Default to "Active" if status is null (but not if empty)
            if (status == null)
                status = "Active";

            var staff = _context.ClinicalStaffs.AsQueryable();

            // Filter only if not empty
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    staff = staff.Where(s => s.IsActive);
                else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    staff = staff.Where(s => !s.IsActive);
                // If empty string, do not apply filter (i.e. All)
            }

            ViewBag.Status = status;

            return View("Index", staff.ToList());
        }

        //Action to sort the staff from new to old
        public IActionResult SortBy(string sortOrder)
        {
            // Toggle sort order: default to newest if not specified
            ViewBag.SortOrder = sortOrder == "oldest" ? "newest" : "oldest";

            // Fetch and sort clinical staff
            var clinicalStaff = _context.ClinicalStaffs.AsQueryable();
            clinicalStaff = sortOrder == "oldest"
                ? clinicalStaff.OrderBy(s => s.ClinicalStaffID) // Oldest to newest
                : clinicalStaff.OrderByDescending(s => s.ClinicalStaffID); // Newest to oldest

            return View("Index", clinicalStaff.ToList());
        }

        //Action to add a new clinical staff member Step 1
        public IActionResult AddNewClinicalStaff()
        {
            // This action method will return the view for adding a new clinical staff member.
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

            //Locally upload the photo before saving to session
            model.Filename = _uploadPhotoServices.UploadPhoto(model.ImageProfile);

            //Upload the Photo to cloudinary
            //Temp URL for profile image
            string tempUrl = string.Empty;

            //Save Image to Cloudinary
            if (!string.IsNullOrEmpty(model.Filename))
            {
                //Convert the relative path to absolute path
                string localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", model.Filename.TrimStart('/'));

                //Check if the file exists
                if (!System.IO.File.Exists(localPath))
                {
                    TempData["Error"] = "Profile image file not found. Please upload a valid image.";
                    return RedirectToAction("AddPatientStep2");
                }

                //Upload the image to Cloudinary
                try
                {
                    //Open the file stream for the image
                    var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

                    //Upload the image using Cloudinary service
                    string photoUrl = _cloudinaryServices.UploadImageAsync(fileStream, Path.GetFileName(localPath)).Result;
                    //Set the PhotoUrl in step1
                    tempUrl = photoUrl;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error" + ex.Message);
                    TempData["Error"] = "Failed to upload profile image: " + ex.Message;
                    return View();
                }
            }


            //Saving Address
            var address = new Address
            {
                House_Unit = model.House_Unit,
                Street = model.Street,
                Subdivision_Village = model.Subdivision_Village,
                Barangay = model.Barangay,
                City = model.City,
                Province = model.Province
            };

            //Save Clinical staff to database
            try
            {
                //Save first the address to database to get the address ID
                 _context.Addresses.Add(address);
                 _context.SaveChanges();


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
                    AddressID = address.AddressID
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
            //Remove Temp Image
            _uploadPhotoServices.DeletePhoto(model.Filename);


            return RedirectToAction("Index");
        }



        //Action to add profile pic
        public IActionResult AddProfilePhoto()
        {
            //Returns the view for adding a profile photo.
            return View();
        }
    }
}
