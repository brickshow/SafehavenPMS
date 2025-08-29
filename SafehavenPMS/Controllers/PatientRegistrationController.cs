using Microsoft.AspNetCore.Mvc;

namespace SafehavenPMS.Controllers
{
    public class PatientRegistrationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
        
}