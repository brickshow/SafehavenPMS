using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class BillableController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private const string TempBillablesSessionKey = "TempBillableOrders";

        public BillableController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}