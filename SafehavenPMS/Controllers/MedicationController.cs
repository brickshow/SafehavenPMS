using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;

namespace SafehavenPMS.Controllers
{
    public class MedicationController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public MedicationController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //View for Add medicine
        public IActionResult AddMedicine()
        {
            return View();
        }
    }
}
