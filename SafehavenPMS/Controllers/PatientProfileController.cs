using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;

namespace SafehavenPMS.Controllers
{
    public class PatientProfileController : Controller
    {
        private readonly SafehavenPMSContext _context;

        //Constructor
        public PatientProfileController(SafehavenPMSContext context)
        {
            _context=context;
        }
        //Action to load the overview tab
        public ActionResult Overview() => PartialView("_Overview");
        //public ActionResult 

        public IActionResult Index(int id)
        {
            var model = GetPatientByID(id);

            // This action method will return the view for the patient profile. Main View
            return View(model);
        }

        //Action to load tabs
        public IActionResult LoadTab(string tab, int patientId)
        {
            var model = GetPatientByID(patientId);
            return PartialView(tab, model); // tab should match the partial view name
        }

        //Method to get patient ID
        private Patient GetPatientByID(int patientID)
        {
            return _context.Patients
                .Include(p => p.Religion)
                .Include(p => p.Nationality)
                .Include(p => p.Address)
                .Include(p => p.MaritalStatus)
                .Include(p => p.EducationLevel)
                .Include(p => p.PatientCases)
                .FirstOrDefault(p => p.PatientId == patientID);
        }
    }
}
