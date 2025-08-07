using Microsoft.AspNetCore.Mvc;

namespace SafehavenPMS.Controllers
{
    public class MedicationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
