using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class PatientController : Controller
    {
        //Inject the SafehavenPMSContext to access the database
        private readonly SafehavenPMSContext _context;
        private readonly UploadPhotoServices _uploadPhotoServices;
        private readonly CloudinaryServices _cloudinaryServices;

        //Constructor to initialize the context
        public PatientController(SafehavenPMSContext safehavenPMSContext)
        {
            // Constructor logic if needed
            _context = safehavenPMSContext;
            _uploadPhotoServices = new UploadPhotoServices();

            _cloudinaryServices = new CloudinaryServices(
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
            );
        }

        public async Task<IActionResult> Index(int? page = 1, int? pageSize = 10, string searchQuery = null, string status = null, string sortOrder = null)
        {
            var query = _context.Patients
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .AsQueryable();

            // Total count (unfiltered)
            int totalPatientCount = await _context.Patients.CountAsync();
            ViewBag.TotalPatientCount = totalPatientCount;
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // Apply search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery)); // Optional
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.PatientStatus == "Active");
                else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.PatientStatus == "Inactive");
                else if (status.Equals("Waitlisted", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.PatientStatus == "Waitlisted");
                else if (status.Equals("Pending Assesment", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.PatientStatus == "Pending Assesment");
                else if (status.Equals("Pending Approval", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.PatientStatus == "Pending Approval");
                else if (status.Equals("Admitted", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.PatientStatus == "Admitted");

            }

            // Apply sorting
            // Apply sorting
            if (sortOrder == null)
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                    : query.OrderByDescending(p => p.Firstname).ThenByDescending(p => p.Lastname);
            }


            // Pagination logic
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            return View(patientList);
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

        [HttpGet]
        public IActionResult FilterStatus(string status)
        {
            return RedirectToAction("Index", new
            {
                status,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                searchQuery = ViewBag.SearchQuery,
                sortOrder = ViewBag.SortOrder
            });
        }

        [HttpGet]
        public IActionResult SortBy(string sortOrder)
        {
            return RedirectToAction("Index", new
            {
                sortOrder,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                searchQuery = ViewBag.SearchQuery,
                status = ViewBag.Status
            });
        }


        //Patient Details
        public IActionResult PatientProfile()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddNewPatient()
        {
            // Extract Physicians from ClinicalStaffs table where Position is "Physician"
            var physicians = await _context.ClinicalStaffs
                .Where(p => p.Position == "Physician") // Corrected filter
                .Select(p => new SelectListItem
                {
                    Value = p.ClinicalStaffID.ToString(), // ClinicalStaffID is the primary key
                    Text = $"{p.Firstname} {p.Lastname}" // Name (or FullName) for display
                })
                .ToListAsync();

            // Check if any physicians were found
            if (!physicians.Any())
            {
                TempData["Message"] = "No Physicians are Listed";
            }

            // Pass the physicians to the view as a SelectList
            ViewBag.Physicians = new SelectList(physicians, "Value", "Text");
            return View();
        }

        // Post action for adding a new patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewPatient(AddNewPatientViewModel model)
        {
            // Remove ImageURL from validation since it's handled manually
            ModelState.Remove("PhotoUrl");

            // Check model validation
            if (!ModelState.IsValid)
            {
                // Log validation errors to console
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                    }
                }

                // Reload physician list for dropdown if validation fails
                var physicians = await _context.ClinicalStaffs
                    .Where(p => p.Position == "Physician")
                    .Select(p => new SelectListItem
                    {
                        Value = p.ClinicalStaffID.ToString(),
                        Text = $"{p.Firstname} {p.Lastname}"
                    })
                    .ToListAsync();

                ViewBag.Physicians = new SelectList(physicians, "Value", "Text", model.ClinicalStaff);
                return View(model);
            }

            //Ensuring BOD not a future 
            if(model.DateOfBirth > DateTime.Now)
            {
                TempData["Message"] = "Please provide a valid Birthdate";
                return View();
            }

            string filename = null;
            string tempUrl = string.Empty;

            // Upload photo locally if provided
            if (model.Filename != null)
            {
                filename = _uploadPhotoServices.UploadPhoto(model.Filename);
            }

            // Upload photo to Cloudinary if a valid local file exists
            if (!string.IsNullOrEmpty(filename))
            {
                string localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filename.TrimStart('/'));

                // If local file not found, return with error
                if (!System.IO.File.Exists(localPath))
                {
                    TempData["Error"] = "Profile image file not found. Please upload a valid image.";

                    // Reload physician list and return view
                    var physicians = await _context.ClinicalStaffs
                        .Where(p => p.Position == "Physician")
                        .Select(p => new SelectListItem
                        {
                            Value = p.ClinicalStaffID.ToString(),
                            Text = $"{p.Firstname} {p.Lastname}"
                        })
                        .ToListAsync();

                    ViewBag.Physicians = new SelectList(physicians, "Value", "Text", model.ClinicalStaff);
                    return View(model);
                }

                // Attempt to upload to Cloudinary
                try
                {
                    using (var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                    {
                        tempUrl = await _cloudinaryServices.UploadImageAsync(fileStream, Path.GetFileName(localPath));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading to Cloudinary: {ex.Message}");
                    TempData["Error"] = $"Failed to upload profile image: {ex.Message}";

                    // Reload physician list and return view
                    var physicians = await _context.ClinicalStaffs
                        .Where(p => p.Position == "Physician")
                        .Select(p => new SelectListItem
                        {
                            Value = p.ClinicalStaffID.ToString(),
                            Text = $"{p.Firstname} {p.Lastname}"
                        })
                        .ToListAsync();

                    ViewBag.Physicians = new SelectList(physicians, "Value", "Text", model.ClinicalStaff);
                    return View(model);
                }
            }

       

           

            try
            {
                // Create new patient object
                var patient = new Patient
                {
                    Firstname = model.Firstname,
                    MiddleName = model.MiddleName,
                    Lastname = model.Lastname,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.ContactNumber,
                    Sex = model.Sex,
                    Occupation = model.Occupation,
                    PatientStatus = "Waitlisted",
                    Education = model.Education,
                    Religion = model.Religion,
                    MaritalStatus = model.MaritalStatus,
                    DateOfReferral = model.DateOfReferral,
                    ReferredBy = model.ReferredBy,
                    Affiliation = model.Affiliation,
                    PhotoUrl = tempUrl,
                    Address = $"{model.House_Unit}, {model.Street}, {model.Subdivision_Village}, {model.Barangay}, {model.City}, {model.Province}",
                    CreatedAt = DateTime.Now,
                };

                // Save patient to database
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // Save physician-patient relationship if selected
                if (model.ClinicalStaff > 0)
                {
                    var clinicalStaffPatient = new ClinicalStaffPatient
                    {
                        PatientId = patient.PatientId,
                        ClinicalStaffId = model.ClinicalStaff
                    };

                    _context.ClinicalStaffPatients.Add(clinicalStaffPatient);
                    await _context.SaveChangesAsync(); // Save the relationship
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving patient: {ex.Message}");
                TempData["Error"] = "There was an error saving the patient.";

                // Reload physician list and return view
                var physicians = await _context.ClinicalStaffs
                    .Where(p => p.Position == "Physician")
                    .Select(p => new SelectListItem
                    {
                        Value = p.ClinicalStaffID.ToString(),
                        Text = $"{p.Firstname} {p.Lastname}"
                    })
                    .ToListAsync();

                ViewBag.Physicians = new SelectList(physicians, "Value", "Text", model.ClinicalStaff);
                return View(model);
            }

            // Delete temp local photo after successful upload
            if (!string.IsNullOrEmpty(filename))
            {
                _uploadPhotoServices.DeletePhoto(filename);
            }

            // Redirect to patient index on success
            return RedirectToAction("Index");
        }

        //Action to proceed Profile
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            //Query staff Information
            var patient = await _context.Patients
                        .Include(p => p.ClinicalStaffPatients)
                            .ThenInclude(csp => csp.ClinicalStaff)
                        .FirstOrDefaultAsync(p => p.PatientId == id); // or however you get the patient


            //Check if staff is not null
            if (patient == null)
            {
                TempData["Error"] = "Staff not found";
            }

            // Build the view model
            var viewModel = new ClinicalStaffProfileViewModel
            {
                // Staffs: all staff assigned to this patient
                Staffs = patient.ClinicalStaffPatients
                        .Select(csp => csp.ClinicalStaff)
                        .Distinct()
                        .ToList(),

                // Patients: just one patient in this case
                Patients = new List<Patient> { patient }
            };

            //Return to View 
            return View(viewModel);
        }

        public IActionResult Edit(int Id)
        {

            return View();
        }
    }
}
