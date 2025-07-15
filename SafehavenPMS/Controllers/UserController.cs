using Microsoft.AspNetCore.Mvc;

namespace SafehavenPMS.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //Add user step1 

        public IActionResult NewUser()
        {
            return View();
        }
    }
}
