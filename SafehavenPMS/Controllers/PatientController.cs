using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations;
using SafehavenPMS.Helpers;
using SafehavenPMS.ViewModel;
using System.Text.Json;

namespace SafehavenPMS.Controllers
{
    public class PatientController : Controller
    {
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
            return View();
        }

        //Action post for step 1
        [HttpPost]
        public IActionResult AddPatientStep1(AddPatientStep1ViewModel model)
        {
            //Check if the model state is valid
            if (!ModelState.IsValid)
            {
                return View(model); // If the model state is invalid, return the view with the model to show validation errors
            }

            // Store the data in Session
            HttpContext.Session.SetObject("AddPatientStep1", model);

            Console.WriteLine("Patient Step 1 Data: " + JsonSerializer.Serialize(model));
            // Redirect to step 2
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

            //Check Step 1 is valid
            if(step1 == null)
            {
                TempData["Error"] = "Step 1 data is missing. Please complete step 1 first";
                return RedirectToAction("AddPatientStep1");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //If step 1 is valid get the image URL
            //TODO: Use Cloudinary or google photos 

            //Check if step 1 data is not null
            return RedirectToAction("AddPatientStep3");
        }

        //Action View for step 3
        public IActionResult AddPatientStep3()
        {
            return View();
        }

        //Action view for step 4
        public IActionResult AddPatientStep4()
        {
            return View();
        }

    }
}
