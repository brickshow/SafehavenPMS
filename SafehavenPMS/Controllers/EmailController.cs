using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Controllers
{
[Authorize]
    public class EmailController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

