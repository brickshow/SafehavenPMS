using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;


namespace SafehavenPMS.Controllers
{
    public class AssessmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}