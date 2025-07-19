using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        private readonly CloudinaryServices _cloudinaryServices;

        //Constructor to initialize the context
        public PatientController(SafehavenPMSContext safehavenPMSContext)
        {
            // Constructor logic if needed
            _context = safehavenPMSContext;
            
            _cloudinaryServices = new CloudinaryServices(
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
            );
        }

        public IActionResult Index()
        {
            return View();
        }

        //Patient Details
        public IActionResult PatientProfile()
        {
            return View();
        }

        //Action View for adding new patient
        public IActionResult AddPatientStep1()
        {
            //Repopulate the data for editing
            var model = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");

            // If session is null, create a new one
            if (model == null)
            {
                model = new AddPatientStep1ViewModel();
            }


            //Display the choices

            // Initialize the SelectListItems for Education Levels and Marital Statuses
            model.EducationLevels = _context.EducationLevels.Select(e => new SelectListItem
            {
                Value = e.EducationLevelId.ToString(),
                Text = e.EducationLevelName
            }).ToList();

            // Initialize the SelectListItems for Marital Statuses
            model.MaritalStatuses = _context.MaritalStatuses.Select(m => new SelectListItem
            {
                Value = m.MaritalStatusId.ToString(),
                Text = m.MaritalStatusType.ToString()
            }).ToList();

            //Initialize the SelectListItems for Religions
            model.Religions = _context.Religions.Select(r => new SelectListItem
            {
                Value = r.ReligionID.ToString(),
                Text = r.ReligionName
            }).ToList();

            //Initialize the SelectListItems for Nationalities
            model.Nationalities = _context.Nationalities.Select(n => new SelectListItem
            {
                Value = n.NationalityID.ToString(),
                Text = n.NationalityName
            }).ToList();
 
            return View(model);
        }

        //This post action already works

        [HttpPost]
        public IActionResult AddPatientStep1(AddPatientStep1ViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Log validation errors
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }

                    // Repopulate all dropdown lists
                    model.EducationLevels = _context.EducationLevels.Select(e => new SelectListItem
                    {
                        Value = e.EducationLevelId.ToString(),
                        Text = e.EducationLevelName
                    }).ToList() ?? new List<SelectListItem>();

                    model.MaritalStatuses = _context.MaritalStatuses.Select(m => new SelectListItem
                    {
                        Value = m.MaritalStatusId.ToString(),
                        Text = m.MaritalStatusType
                    }).ToList() ?? new List<SelectListItem>();

                    model.Religions = _context.Religions.Select(r => new SelectListItem
                    {
                        Value = r.ReligionID.ToString(),
                        Text = r.ReligionName
                    }).ToList() ?? new List<SelectListItem>();

                    model.Nationalities = _context.Nationalities.Select(n => new SelectListItem
                    {
                        Value = n.NationalityID.ToString(),
                        Text = n.NationalityName
                    }).ToList() ?? new List<SelectListItem>();

                    // Return the view with the repopulated model
                    return View(model);
                }


                //Get the Name of each ID's
                //Get the name for Religion
                model.ReligionName = _context.Religions
                    .Where(r => r.ReligionID == model.ReligionId)
                    .Select(re => re.ReligionName)
                    .FirstOrDefault()?? " ";

                //Query the name for Education
                model.EducationName = _context.EducationLevels
                        .Where(e => e.EducationLevelId == model.EducationLevelId)
                        .Select(e => e.EducationLevelName)
                        .FirstOrDefault() ?? "";

                //Query the name for Marital Status
                model.MaritalStatusName = _context.MaritalStatuses
                    .Where(m => m.MaritalStatusId == model.MaritalStatusId)
                    .Select(ma => ma.MaritalStatusType)
                    .FirstOrDefault() ?? "";

                //Query the name for Nationality
                model.NationalityName = _context.Nationalities
                    .Where(n => n.NationalityID == model.NationalityId)
                    .Select(na => na.NationalityName)
                    .FirstOrDefault() ?? "";

                // Serialize the model to JSON and log it
                var json = System.Text.Json.JsonSerializer.Serialize(model);
                Console.WriteLine("Storing AddPatientStep1 in session: " + json);


                // Store the data in Session
                HttpContext.Session.SetObject("AddPatientStep1", model);

                //Redirect to Step 2
                return RedirectToAction("AddPatientStep2");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Repopulate dropdowns here too in case of exception
                return View(model);
            }
        }

        //Action View For Step 2
        public IActionResult AddPatientStep2()
        {
            var model = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2") ?? new AddPatientStep2ViewModel();
            return View(model);
        }

        //Action Post for step 2
        [HttpPost]
        public IActionResult AddPatientStep2(AddPatientStep2ViewModel model)
        {
            // Retrieve and check data from step 1
            var step1 = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");

            Console.WriteLine(JsonSerializer.Serialize(step1));

            // Check Step 1 is valid
            if (step1 == null)
            {
                TempData["Error"] = "Step 1 data is missing. Please complete step 1 first";
                return RedirectToAction("AddPatientStep1");
            }

            //Load the previously stored step 2 if exists
            var existingStep2 = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2");

            if (!ModelState.IsValid)
            {
                //Preserve existing image path in case of validitaion error
                if (existingStep2 != null && string.IsNullOrEmpty(model.TempFilePath))
                {
                    //Save the temp path
                    model.TempFilePath = existingStep2.TempFilePath;
                }
                return View(model);
            }

            string tempFilePath = string.Empty;

            // Check if the profile image is provided
            if(model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                //Get Upload path
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "patientProfileImages");

                // Ensure the upload folder exists
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Create a unique file name for the uploaded image
                var fileName = $"{Guid.NewGuid()}_{model.ProfileImage.FileName}";
                tempFilePath = Path.Combine(uploadFolder, fileName); //Example: wwwroot/uploads/patientProfileImages/unique_filename.jpg

                //Save the uploaded image to the temporary file path
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    model.ProfileImage.CopyTo(fileStream);
                }

                //Store path in session
                model.TempFilePath = $"/uploads/patientProfileImages/{fileName}";

                // Log the temporary file path
                Console.WriteLine("Temporary file path: " + fileName);
            }
            else
            {
                // Restore previous image if no new image is uploaded
                if (existingStep2 != null)
                {
                    model.TempFilePath = existingStep2.TempFilePath;
                }
            }


            // Store the step 2 data in Session
            HttpContext.Session.SetObject("AddPatientStep2", model);

            //
            return RedirectToAction("AddPatientStep3");
        }

        //Action View for step 3
        public IActionResult AddPatientStep3()
        {
            //Populate Step 3 with data for Editing
            var model = HttpContext.Session.GetObject<AddPatientStep3ViewModel>("AddPatientStep3");
            return View(model);
        }

        //Action Method to upload case details
        [HttpPost]
        public IActionResult AddPatientStep3(AddPatientStep3ViewModel model)
        {
            //Decode the Step1 session
            var step1 = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");
            Console.WriteLine("Step 1 data" + JsonSerializer.Serialize(step1));

            //Decode the step2 session
            var step2 = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2");
            Console.WriteLine("Step 2 data" + JsonSerializer.Serialize(step2));


            //Check if step1 and 2 is empty
            if (step1 == null || step2 == null)
            {
                TempData["Error"] = "Patient Information is empty";
                return View("AddPatientStep1");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            //Add data to json
            HttpContext.Session.SetObject("AddPatientStep3", model);

            //Check in console if data are present
            Console.WriteLine("Patient Step 1 data: " + JsonSerializer.Serialize(step1));
            Console.WriteLine("Patient Step 2 data: " + JsonSerializer.Serialize(step2));
            Console.WriteLine("Patient Step 3 data: " + JsonSerializer.Serialize(model));

            //Save the patient data to the model
            var confirmationModel = new ConfirmationViewModel
            {
                Step1 = step1,
                Step2 = step2,
                Step3 = model
            };

            //Pass the confirmation model to the view
            ViewBag.ConfirmationModel = confirmationModel;
            ViewBag.ShowModal = true;

            //Redirect to Confirmation
            return View(model);
        }

        //Action for confirmation before saving to database
        public IActionResult Confirmation()
        {
            return View();
        }

        //Action to confirm adding patient and save to database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveConfirmation()
        {
            //Retrieve Session data
            var step1 = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");
            var step2 = HttpContext.Session.GetObject<AddPatientStep2ViewModel>("AddPatientStep2");
            var step3 = HttpContext.Session.GetObject<AddPatientStep3ViewModel>("AddPatientStep3");

            //Check if it is valid
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AddPatientStep4");
            }

            //Generate Unique ID for Patient
            int id = 000000;

            //Check id session is null
            if(step1 == null || step2 == null)
            {
                //Return and throw error
                TempData["Error"] = "Some of the required patient data is missing. Please re-enter.";
                return RedirectToAction("AddPatientStep1");
            }

            //Add the address first to context
            var address = new Address
            {
                Street = step1.Street,
                Barangay = step1.Barangay,
                City = step1.City,
                Province = step1.Province,
                Country = step1.Country,
                PostalCode = step1.PostalCode,
            };

            //Temp URL for profile image
            string tempUrl = string.Empty;

            //Save Image to Cloudinary
            if (!string.IsNullOrEmpty(step2.TempFilePath))
            {
                //Convert the relative path to absolute path
                string localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", step2.TempFilePath.TrimStart('/'));

                //Check if the file exists
                if(!System.IO.File.Exists(localPath))
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
                    TempData["Error"] = "Failed to upload profile image: " + ex.Message;
                    return RedirectToAction("AddPatientStep2");
                }
            }

            //Save the Patient to the database
            try
            {
                // Step 1: Add the address and save it first
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync(); // AddressID is generated here

                // Step 2: Now create the patient using the saved address ID
                var patient = new Patient
                {
                    EducationLevelID = step1.EducationLevelId,
                    ReligionID = step1.ReligionId,
                    MaritalStatusID = step1.MaritalStatusId,
                    AddressID = address.AddressID, // Now it has a real ID
                    NationalityID = step1.NationalityId,

                    Firstname = step1.Firstname,
                    Lastname = step1.Lastname,
                    MiddleName = step1.MiddleName,
                    ContactNumber = step1.ContactNumber,
                    Sex = step1.Sex,
                    DateOfBirth = step1.DateOfBirth,
                    PatienStatus = "Waiting List",
                    Occupation = step1.Occupation,
                    PhotoUrl = tempUrl
                };

                // Step 3: Save the patient
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // Clear session
                HttpContext.Session.Remove("AddPatientStep1");
                HttpContext.Session.Remove("AddPatientStep2");
                HttpContext.Session.Remove("AddPatientStep3");

                TempData["Success"] = "Patient added successfully!";
                return RedirectToAction("AddPatientStep4");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving patient: " + ex.Message);
                TempData["Error"] = "There was an error saving the patient.";
                return View(); // Make sure there's a view for fallback here
            }

        }

        //Action view for step 4
        public IActionResult AddPatientStep4()
        {
            return View();
        }

    }
}
