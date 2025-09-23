using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.ViewModel.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class BillingController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public BillingController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var billable = await _context.MedicationOrders
                                .Include(p => p.Patient)
                                .Include(m => m.Medicine)
                                .ToListAsync();

            var billableItems = billable.Select(b => new BillableItemViewModel
            {
                PatientId = b.PatientId,
                PatientName = b.Patient != null ? $"{b.Patient.Firstname} {b.Patient.Lastname}" : null,
                MedicationId = b.MedicineId,
                Category = "Medication",
                Description = $"{b.Medicine?.GenericName}",
                Quantity = b.UnitPerDose,
                UnitPrice = b.Medicine?.Price ?? 0m,
                DateAdded = b.CreatedAt,
                CreatedBy = b.CreatedBy
            }).ToList();
            
            var viewModel = new BillablesPageViewModel
            {
                Items = billableItems
            };

            return View(viewModel);
        }
    }
}