using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;

namespace SafehavenPMS.Controllers
{
    public class ClinicalStaffController : Controller
    {
        //Inject Context or services if needed
        private readonly SafehavenPMSContext _context;

        //Constructor to initialize the context
        public ClinicalStaffController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Action to add a new clinical staff member Step 1
        public IActionResult AddClinicalStaffStep1()
        {
            // This action method will return the view for adding a new clinical staff member.
            return View();
        }
    }
}
