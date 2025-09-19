using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;

namespace SafehavenPMS.Controllers
{
    public class ProgressNoteController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public ProgressNoteController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Logic to retrieve and display progress notes can be added here
            return View();
        }

        public IActionResult Create()
        {
            // Logic to create a new progress note can be added here
            return View();
        }
    }
}