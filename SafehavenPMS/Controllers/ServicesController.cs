using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Controllers
{
[Authorize]
    public class ServicesController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public ServicesController(SafehavenPMSContext context)
        {
            _context = context;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Where(s => !s.IsArchived)
                .ToListAsync();

            ViewBag.ServiceTypes = new SelectList(await _context.ServiceTypes.ToListAsync(), "ServiceTypeId", "ServiceName");
            ViewBag.TotalServices = services.Count;
            return View(services);
        }

        // POST: Services/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string? serviceName, string? assignedRole)
        {
            if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(assignedRole))
            {
                TempData["ErrorMessage"] = "Service name and assigned role are required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var newService = new Service
                {
                    ServiceName = serviceName,
                    AssignedRole = assignedRole,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "User" // Replace with actual user
                };

                _context.Services.Add(newService);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service added successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to add service. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Services/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int serviceId, string serviceName, string assignedRole)
        {
            if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(assignedRole))
            {
                TempData["ErrorMessage"] = "Service name and assigned role are required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                {
                    TempData["ErrorMessage"] = "Service not found.";
                    return RedirectToAction(nameof(Index));
                }

                service.ServiceName = serviceName;
                service.AssignedRole = assignedRole;
                service.UpdatedAt = DateTime.Now;
                service.UpdatedBy = "User"; // Replace with actual user

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update service. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Services/Archive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int serviceId)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                {
                    TempData["ErrorMessage"] = "Service not found.";
                    return RedirectToAction(nameof(Index));
                }

                service.IsArchived = true;
                service.UpdatedAt = DateTime.Now;
                service.UpdatedBy = "User"; // Replace with actual user

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service archived successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to archive service. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
