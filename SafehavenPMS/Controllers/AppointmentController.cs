using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;
using SafehavenPMS.ViewModel;

namespace SafehavenPMS.Controllers
{
    public class AppointmentController : Controller
    {
        //Inject Context or services if needed
        private readonly SafehavenPMSContext _context;

        //Constructor
        public AppointmentController(SafehavenPMSContext context)
        {
            _context = context;
        }

        //Action for adding Clinical staff availability
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult AddAvailability(AvailabilityViewModel model)
        {

            return PartialView("_Availability");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
