using Microsoft.AspNetCore.Mvc;

namespace SafehavenPMS.Controllers
{
    public class AppointmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
