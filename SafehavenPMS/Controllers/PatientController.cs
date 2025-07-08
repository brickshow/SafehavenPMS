using Microsoft.AspNetCore.Mvc;
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Store the data in TempData for step 2
            TempData["PatientStep1"] = JsonSerializer.Serialize(model);

            // Redirect to step 2
            return RedirectToAction("AddPatientStep2");
        }

        //Action View For Step 2
        public IActionResult AddPatientStep2()
        {
            // Check if we have data from step 1
            if (TempData["PatientStep1"] == null)
            {
                return RedirectToAction("AddPatientStep1");
            }

            Console.WriteLine(TempData["PatientStep1"]);

            return View();
        }
        //Acition Post for step 2
        [HttpPost]
        public IActionResult AddPatientStep2(AddPatientStep2ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Store the data in TempData for step 3
            TempData["PatientStep2"] = JsonSerializer.Serialize(model);
            return RedirectToAction("AddPatientStep3");
        }

        //Action View for step 3
        public IActionResult AddPatientStep3()
        {
            if (TempData["PatientStep2"] == null)
            {
                return RedirectToAction("AddPatientStep1");
            }
            return View();
        }

    }
}
