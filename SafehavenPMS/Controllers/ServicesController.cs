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
        public async Task<IActionResult> Index(
            string searchQuery = null,
            string sortBy = null,
            string sortOrder = null,
            int page = 1,
            int pageSize = 10)
        {
            // normalize
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder.ToLower();
            bool asc = sortOrder == "ascending";

            var query = _context.Services
                .Where(s => !s.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var s = searchQuery.Trim().ToLower();
                query = query.Where(x =>
                    (x.ServiceName ?? "").ToLower().Contains(s) ||
                    (x.AssignedRole ?? "").ToLower().Contains(s));
            }

            // sorting: default by CreatedAt if none, else by Name
            if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
            {
                query = asc ? query.OrderBy(x => x.ServiceName) : query.OrderByDescending(x => x.ServiceName);
            }
            else
            {
                query = asc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (pageSize > 0) ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            List<Service> services;
            if (pageSize == 0)
            {
                services = await query.ToListAsync();
            }
            else
            {
                services = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            }

            // viewbags for view state
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortBy = sortBy ?? "";
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalServices = totalItems;

            ViewBag.ServiceTypes = new SelectList(await _context.ServiceTypes.ToListAsync(), "ServiceTypeId", "ServiceName");
            return View(services);
        }

        [HttpGet]
        public IActionResult SortBy(string sortBy, string sortOrder, int page = 1, int pageSize = 10)
        {
            return RedirectToAction("Index", new { sortBy, sortOrder, page, pageSize });
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
