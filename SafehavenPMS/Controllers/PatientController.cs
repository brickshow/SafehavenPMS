using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Migrations;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System.Text.Json;

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
            //Display the choices
            var model = new AddPatientStep1ViewModel
            {
                // Initialize the SelectListItems for Education Levels and Marital Statuses
                EducationLevels = _context.EducationLevels.Select(e => new SelectListItem
                {
                    Value = e.EducationLevelId.ToString(),
                    Text = e.EducationLevelName
                }).ToList(),

                // Initialize the SelectListItems for Marital Statuses
                MaritalStatuses = _context.MaritalStatuses.Select(m => new SelectListItem
                {
                    Value = m.MaritalStatusId.ToString(),
                    Text = m.MaritalStatusType.ToString()
                }).ToList(),

                //Initialize the SelectListItems for Religions
                Religions = _context.Religions.Select(r => new SelectListItem
                {
                    Value = r.ReligionID.ToString(),
                    Text = r.ReligionName
                }),

                //Initialize the SelectListItems for Nationalities
                Nationalities = _context.Nationalities.Select(n => new SelectListItem
                {
                    Value = n.NationalityID.ToString(),
                    Text = n.NationalityName
                })
            };
            return View(model);
        }

        //Action post for step 1
        [HttpPost]
        public IActionResult AddPatientStep1(AddPatientStep1ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns
                model.EducationLevels = _context.EducationLevels.Select(e => new SelectListItem
                {
                    Value = e.EducationLevelId.ToString(),
                    Text = e.EducationLevelName
                }).ToList();

                model.MaritalStatuses = _context.MaritalStatuses.Select(m => new SelectListItem
                {
                    Value = m.MaritalStatusId.ToString(),
                    Text = m.MaritalStatusType
                }).ToList();

                model.Religions = _context.Religions.Select(r => new SelectListItem
                {
                    Value = r.ReligionID.ToString(),
                    Text = r.ReligionName
                }).ToList();

                model.Nationalities = _context.Nationalities.Select(n => new SelectListItem
                {
                    Value = n.NationalityID.ToString(),
                    Text = n.NationalityName
                }).ToList();
                return View(model);
            }

            // Store the data in Session
            HttpContext.Session.SetObject("AddPatientStep1", model);

            Console.WriteLine("Patient Step 1 Data: " + JsonSerializer.Serialize(model));
            return RedirectToAction("AddPatientStep2");
        }

        //Action View For Step 2
        public IActionResult AddPatientStep2()
        {
            return View();
        }

        //Action Post for step 2
        [HttpPost]
        public IActionResult AddPatientStep2(AddPatientStep2ViewModel model)
        {
            // Retrieve and check data from step 1
            var step1 = HttpContext.Session.GetObject<AddPatientStep1ViewModel>("AddPatientStep1");

            // Check Step 1 is valid
            if (step1 == null)
            {
                TempData["Error"] = "Step 1 data is missing. Please complete step 1 first";
                return RedirectToAction("AddPatientStep1");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // If step 1 is valid get the image URL
            var imageUrl = string.Empty;
            var file = model.ProfileImage;

            // Check if the file is not null and has content
            if (file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    // Upload the image to Cloudinary and get the URL
                    imageUrl = _cloudinaryServices.UploadImageAsync(stream, file.FileName).Result;
                }
            }

            // Pass the data to the next step
            var step2Data = new
            {
                ProfileImageUrl = imageUrl,
                model.ProfileImage
            };

            Console.WriteLine(imageUrl);


            // Store the step 2 data in Session
            HttpContext.Session.SetObject("AddPatientStep2", step2Data);

            Console.WriteLine("Patient Step 2 Data: " + JsonSerializer.Serialize(step2Data));

            return RedirectToAction("AddPatientStep3");
        }

        //Action View for step 3
        public IActionResult AddPatientStep3()
        {
            return View();
        }

        ////Action Method to upload case details
        //[HttpPost]
        //public IActionResult AddPatientStep3()
        //{

        //}

        //Action view for step 4
        public IActionResult AddPatientStep4()
        {
            return View();
        }

    }
}
