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
                .Include(p => p.Address)
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

            // Create address object from model input
            var address = new Address
            {
                House_Unit = model.House_Unit,
                Street = model.Street,
                Subdivision_Village = model.Subdivision_Village,
                Barangay = model.Barangay,
                City = model.City,
                Province = model.Province
            };

            // Save address to database
            try
            {
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving address: {ex.Message}");
                TempData["Error"] = "There was an error saving the address.";

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
                    AddressID = address.AddressID,
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

        //    //Action View for adding new patient
        //    public IActionResult AddPatientStep1()
        //    {
        //        return View();
        //    }

        //    //This post action already works

        //    //[HttpPost]
        //    //public IActionResult AddPatientStep1(AddPatientStep1ViewModel model)
        //    //{
        //    //    try
        //    //    {
        //    //        if (!ModelState.IsValid)
        //    //        {
        //    //            // Log validation errors
        //    //            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        //    //            {
        //    //                Console.WriteLine(error.ErrorMessage);
        //    //            }

        //    //            // Repopulate all dropdown lists
        //    //            model.EducationLevels = _context.EducationLevels.Select(e => new SelectListItem
        //    //            {
        //    //                Value = e.EducationLevelId.ToString(),
        //    //                Text = e.EducationLevelName
        //    //            }).ToList() ?? new List<SelectListItem>();

        //    //            model.MaritalStatuses = _context.MaritalStatuses.Select(m => new SelectListItem
        //    //            {
        //    //                Value = m.MaritalStatusId.ToString(),
        //    //                Text = m.MaritalStatusType
        //    //            }).ToList() ?? new List<SelectListItem>();

        //    //            model.Religions = _context.Religions.Select(r => new SelectListItem
        //    //            {
        //    //                Value = r.ReligionID.ToString(),
        //    //                Text = r.ReligionName
        //    //            }).ToList() ?? new List<SelectListItem>();

        //    //            model.Nationalities = _context.Nationalities.Select(n => new SelectListItem
        //    //            {
        //    //                Value = n.NationalityID.ToString(),
        //    //                Text = n.NationalityName
        //    //            }).ToList() ?? new List<SelectListItem>();

        //    //            // Return the view with the repopulated model
        //    //            return View(model);
        //    //        }


        //    //        //Get the Name of each ID's
        //    //        //Get the name for Religion
        //    //        model.ReligionName = _context.Religions
        //    //            .Where(r => r.ReligionID == model.ReligionId)
        //    //            .Select(re => re.ReligionName)
        //    //            .FirstOrDefault()?? " ";

        //    //        //Query the name for Education
        //    //        model.EducationName = _context.EducationLevels
        //    //                .Where(e => e.EducationLevelId == model.EducationLevelId)
        //    //                .Select(e => e.EducationLevelName)
        //    //                .FirstOrDefault() ?? "";

        //    //        //Query the name for Marital Status
        //    //        model.MaritalStatusName = _context.MaritalStatuses
        //    //            .Where(m => m.MaritalStatusId == model.MaritalStatusId)
        //    //            .Select(ma => ma.MaritalStatusType)
        //    //            .FirstOrDefault() ?? "";

        //    //        //Query the name for Nationality
        //    //        model.NationalityName = _context.Nationalities
        //    //            .Where(n => n.NationalityID == model.NationalityId)
        //    //            .Select(na => na.NationalityName)
        //    //            .FirstOrDefault() ?? "";

        //    //        // Serialize the model to JSON and log it
        //    //        var json = System.Text.Json.JsonSerializer.Serialize(model);
        //    //        Console.WriteLine("Storing AddPatientStep1 in session: " + json);


        //    //        // Store the data in Session
        //    //        HttpContext.Session.SetObject("AddPatientStep1", model);

        //    //        //Redirect to Step 2
        //    //        return RedirectToAction("AddPatientStep2");
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        Console.WriteLine(ex.ToString());
        //    //        // Repopulate dropdowns here too in case of exception
        //    //        return View(model);
        //    //    }
        //    //}

        //    //Action View For Step 2
        //    public IActionResult AddPatientStep2()
        //    {
        //        var model = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2") ?? new AddPatientStep2ViewModel();
        //        return View(model);
        //    }

        //    //Action Post for step 2
        //    [HttpPost]
        //    public IActionResult AddPatientStep2(AddPatientStep2ViewModel model)
        //    {
        //        // Retrieve and check data from step 1
        //        var step1 = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");

        //        Console.WriteLine(JsonSerializer.Serialize(step1));

        //        // Check Step 1 is valid
        //        if (step1 == null)
        //        {
        //            TempData["Error"] = "Step 1 data is missing. Please complete step 1 first";
        //            return RedirectToAction("AddPatientStep1");
        //        }

        //        //Load the previously stored step 2 if exists
        //        var existingStep2 = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2");

        //        if (!ModelState.IsValid)
        //        {
        //            //Preserve existing image path in case of validitaion error
        //            if (existingStep2 != null && string.IsNullOrEmpty(model.TempFilePath))
        //            {
        //                //Save the temp path
        //                model.TempFilePath = existingStep2.TempFilePath;
        //            }
        //            return View(model);
        //        }

        //        string tempFilePath = string.Empty;

        //        // Check if the profile image is provided
        //        if(model.ProfileImage != null && model.ProfileImage.Length > 0)
        //        {
        //            //Get Upload path
        //            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "patientProfileImages");

        //            // Ensure the upload folder exists
        //            if (!Directory.Exists(uploadFolder))
        //            {
        //                Directory.CreateDirectory(uploadFolder);
        //            }

        //            // Create a unique file name for the uploaded image
        //            var fileName = $"{Guid.NewGuid()}_{model.ProfileImage.FileName}";
        //            tempFilePath = Path.Combine(uploadFolder, fileName); //Example: wwwroot/uploads/patientProfileImages/unique_filename.jpg

        //            //Save the uploaded image to the temporary file path
        //            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
        //            {
        //                model.ProfileImage.CopyTo(fileStream);
        //            }

        //            //Store path in session
        //            model.TempFilePath = $"/uploads/patientProfileImages/{fileName}";

        //            // Log the temporary file path
        //            Console.WriteLine("Temporary file path: " + fileName);
        //        }
        //        else
        //        {
        //            // Restore previous image if no new image is uploaded
        //            if (existingStep2 != null)
        //            {
        //                model.TempFilePath = existingStep2.TempFilePath;
        //            }
        //        }


        //        // Store the step 2 data in Session
        //        HttpContext.Session.SetObject("AddPatientStep2", model);

        //        //
        //        return RedirectToAction("AddPatientStep3");
        //    }

        //    //Action View for step 3
        //    public IActionResult AddPatientStep3()
        //    {
        //        //Populate Step 3 with data for Editing
        //        var model = HttpContext.Session.GetObject<AddPatientStep3ViewModel>("AddPatientStep3");
        //        return View(model);
        //    }

        //    //Action Method to upload case details
        //    [HttpPost]
        //    public IActionResult AddPatientStep3(AddPatientStep3ViewModel model)
        //    {
        //        //Decode the Step1 session
        //        var step1 = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");
        //        Console.WriteLine("Step 1 data" + JsonSerializer.Serialize(step1));

        //        //Decode the step2 session
        //        var step2 = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2");
        //        Console.WriteLine("Step 2 data" + JsonSerializer.Serialize(step2));


        //        //Check if step1 and 2 is empty
        //        if (step1 == null || step2 == null)
        //        {
        //            TempData["Error"] = "Patient Information is empty";
        //            return View("AddPatientStep1");
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            return View();
        //        }

        //        //Add data to json
        //        HttpContext.Session.SetObject("AddPatientStep3", model);

        //        //Check in console if data are present
        //        Console.WriteLine("Patient Step 1 data: " + JsonSerializer.Serialize(step1));
        //        Console.WriteLine("Patient Step 2 data: " + JsonSerializer.Serialize(step2));
        //        Console.WriteLine("Patient Step 3 data: " + JsonSerializer.Serialize(model));

        //        //Save the patient data to the model
        //        var confirmationModel = new ConfirmationViewModel
        //        {
        //            Step1 = step1,
        //            Step2 = step2,
        //            Step3 = model
        //        };

        //        //Pass the confirmation model to the view
        //        ViewBag.ConfirmationModel = confirmationModel;
        //        ViewBag.ShowModal = true;

        //        //Redirect to Confirmation
        //        return View(model);
        //    }

        //    //Action for confirmation before saving to database
        //    public IActionResult Confirmation()
        //    {
        //        return View();
        //    }

        //    //Action to confirm adding patient and save to database
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public async Task<IActionResult> SaveConfirmation()
        //    {
        //        //Retrieve Session data
        //        var step1 = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");
        //        var step2 = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2");
        //        var step3 = HttpContext.Session.GetObject<AddPatientStep3ViewModel>("AddPatientStep3");

        //        //Check if it is valid
        //        if (!ModelState.IsValid)
        //        {
        //            return RedirectToAction("AddPatientStep4");
        //        }

        //        //Check id session is null
        //        if(step1 == null || step2 == null)
        //        {
        //            //Return and throw error
        //            TempData["Error"] = "Some of the required patient data is missing. Please re-enter.";
        //            return RedirectToAction("AddPatientStep1");
        //        }

        //        //Add the address first to context
        //        var address = new Address
        //        {
        //            Street = step1.Street,
        //            Barangay = step1.Barangay,
        //            City = step1.City,
        //            Province = step1.Province,
        //        };

        //        //Temp URL for profile image
        //        string tempUrl = string.Empty;

        //        //Save Image to Cloudinary
        //        if (!string.IsNullOrEmpty(step2.TempFilePath))
        //        {
        //            //Convert the relative path to absolute path
        //            string localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", step2.TempFilePath.TrimStart('/'));

        //            //Check if the file exists
        //            if(!System.IO.File.Exists(localPath))
        //            {
        //                TempData["Error"] = "Profile image file not found. Please upload a valid image.";
        //                return RedirectToAction("AddPatientStep2");
        //            }

        //            //Upload the image to Cloudinary
        //            try
        //            {
        //                //Open the file stream for the image
        //                var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

        //                //Upload the image using Cloudinary service
        //                string photoUrl = _cloudinaryServices.UploadImageAsync(fileStream, Path.GetFileName(localPath)).Result;
        //                //Set the PhotoUrl in step1
        //                tempUrl = photoUrl;
        //            }
        //            catch (Exception ex)
        //            {
        //                TempData["Error"] = "Failed to upload profile image: " + ex.Message;
        //                return RedirectToAction("AddPatientStep2");
        //            }
        //        }

        //        //Save the Patient to the database
        //        try
        //        {
        //            // Step 1: Add the address and save it first
        //            _context.Addresses.Add(address);
        //            await _context.SaveChangesAsync(); // AddressID is generated here

        //            // Step 2: Now create the patient using the saved address ID
        //            var patient = new Patient
        //            {
        //                EducationLevelID = step1.EducationLevelId,
        //                ReligionID = step1.ReligionId,
        //                MaritalStatusID = step1.MaritalStatusId,
        //                AddressID = address.AddressID, // Now it has a real ID
        //                NationalityID = step1.NationalityId,

        //                Firstname = step1.Firstname,
        //                Lastname = step1.Lastname,
        //                MiddleName = step1.MiddleName,
        //                ContactNumber = step1.ContactNumber,
        //                Sex = step1.Sex,
        //                DateOfBirth = step1.DateOfBirth,
        //                PatienStatus = "Waiting List",
        //                Occupation = step1.Occupation,
        //                PhotoUrl = tempUrl
        //            };

        //            // Step 3: Save the patient
        //            _context.Patients.Add(patient);
        //            await _context.SaveChangesAsync();

        //            // Clear session
        //            HttpContext.Session.Remove("AddPatientStep1");
        //            HttpContext.Session.Remove("AddPatientStep2");
        //            HttpContext.Session.Remove("AddPatientStep3");

        //            TempData["Success"] = "Patient added successfully!";

        //            return RedirectToAction("AddPatientStep4");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Error saving patient: " + ex.Message);
        //            TempData["Error"] = "There was an error saving the patient.";
        //            return View(); // Make sure there's a view for fallback here
        //        }
        //    }

        //    //Action view for step 4
        //    public IActionResult AddPatientStep4()
        //    {
        //        return View();
        //    }

        //}}
    }
}
