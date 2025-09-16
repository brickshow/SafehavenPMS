using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.ViewModel.ServicesType;
using SafehavenPMS.Models;

namespace SafehavenPMS.Controllers
{
    public class ServiceTypeController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public ServiceTypeController(SafehavenPMSContext context)
        {
            _context = context;
        }

        // GET: ServiceType
        public async Task<IActionResult> Index()
        {
            var serviceTypes = await _context.ServiceTypes.ToListAsync();

            ViewBag.TotalServiceTypes = serviceTypes.Count;
            return View(serviceTypes);
        }

        // POST: ServiceType/AddNewServiceType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewServiceType(string? serviceType, string? description)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var newServiceType = new ServiceType
                {
                    ServiceName = serviceType,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "User" //Change to oroginal User Soon

                };

                _context.ServiceTypes.Add(newServiceType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service Type added successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to add Service Type. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ServiceType/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ServiceTypeId, string ServiceName, string Description)
        {
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                TempData["ErrorMessage"] = "Service Type name is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var serviceType = await _context.ServiceTypes.FindAsync(ServiceTypeId);
                if (serviceType == null)
                {
                    TempData["ErrorMessage"] = "Service Type not found.";
                    return RedirectToAction(nameof(Index));
                }

                serviceType.ServiceName = ServiceName;
                serviceType.Description = Description;
                serviceType.UpdatedAt = DateTime.Now;
                serviceType.UpdatedBy = "User"; // Change to actual user if available

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service Type updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update Service Type. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}