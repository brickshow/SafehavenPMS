using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Helpers;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class PatientController : Controller
    {
        //Inject the SafehavenPMSContext to access the database
        private readonly SafehavenPMSContext _context;
        private readonly UploadPhotoServices _uploadPhotoServices;
        private readonly CloudinaryServices _cloudinaryServices;

        //Constructor to initialize the context
        public PatientController(SafehavenPMSContext safehavenPMSContext)
        {
            // Constructor logic if needed
            _context = safehavenPMSContext;
            _uploadPhotoServices = new UploadPhotoServices();

            _cloudinaryServices = new CloudinaryServices(
                new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
            );
        }

        public async Task<IActionResult> Index(
            int? page = 1,
            int? pageSize = 10,
            string searchQuery = null,
            string status = null,
            string sortOrder = null)
        {
            var query = _context.Patients
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .AsQueryable();

            // Counts for each status
            ViewBag.TotalPatientCount = await _context.Patients.CountAsync();
            ViewBag.WaitlistedCount = await _context.Patients.CountAsync(p => p.PatientStatus == Enum.PatientStatusEnum.Waitlisted.ToString());
            ViewBag.PendingAssessmentCount = await _context.Patients.CountAsync(p => p.PatientStatus == Enum.PatientStatusEnum.PendingAssessment.ToString());
            ViewBag.PendingApprovalCount = await _context.Patients.CountAsync(p => p.PatientStatus == Enum.PatientStatusEnum.PendingApproval.ToString());
            //ViewBag.ActiveCount = await _context.Patients.CountAsync(p => p.PatientStatus == "Active");
            //ViewBag.InactiveCount = await _context.Patients.CountAsync(p => p.PatientStatus == "Inactive");
            //ViewBag.AdmittedCount = await _context.Patients.CountAsync(p => p.PatientStatus == "Admitted");

            // Pass current filters/sorting to view
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // 🔎 Apply search filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery));
            }

            // ✅ Apply status filter (default = All)
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.PatientStatus.ToString() == status);
            }

            // 🔃 Apply sorting
            if (sortOrder == null)
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                query = sortOrder == "ascending"
                    ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                    : query.OrderByDescending(p => p.Firstname).ThenByDescending(p => p.Lastname);
            }

            // 📄 Pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            return View(patientList);
        }

        [HttpGet]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Index", new
            {
                searchQuery,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                status = ViewBag.Status,
                sortOrder = ViewBag.SortOrder
            });
        }

        [HttpGet]
        public IActionResult SortBy(string sortOrder)
        {
            return RedirectToAction("Index", new
            {
                sortOrder,
                page = 1,
                pageSize = ViewBag.PageSize ?? 10,
                searchQuery = ViewBag.SearchQuery,
                status = ViewBag.Status
            });
        }


        //Patient Details
        public IActionResult PatientProfile()
        {
            return View();
        }

        [HttpGet]
        public  IActionResult AddNewPatient()
        {
            return View();
        }

        // Post action for adding a new patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewPatient(AddNewPatientViewModel model)
        {
            ModelState.Remove("PhotoUrl");

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            if (model.DateOfBirth > DateTime.Now)
            {
                TempData["Message"] = "Please provide a valid Birthdate";
                return View();
            }

            string? tempUrl = null;

            if (model.Filename != null && model.Filename.Length > 0)
            {
                try
                {
                    using (var stream = model.Filename.OpenReadStream())
                    {
                        tempUrl = await _cloudinaryServices.UploadImageAsync(stream, model.Filename.FileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading to Cloudinary: {ex.Message}");
                    TempData["Error"] = $"Failed to upload profile image: {ex.Message}";
                    return View(model);
                }
            }

            try
            {
                // Create and save patient first
                var patient = new Patient
                {
                    Firstname = model.Firstname,
                    MiddleName = model.MiddleName ?? string.Empty,
                    Lastname = model.Lastname,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.ContactNumber,
                    Sex = model.Sex,
                    Occupation = model.Occupation ?? string.Empty,
                    PatientStatus = Enum.PatientStatusEnum.NewReferral.ToString(),
                    Education = model.Education ?? string.Empty,
                    Religion = model.Religion ?? string.Empty,
                    MaritalStatus = model.MaritalStatus,
                    PhotoUrl = tempUrl ?? string.Empty,
                    Address = $"{model.House_Unit}, {model.Street}, {model.Subdivision_Village}, {model.Barangay}, {model.City}, {model.Province}",
                    CreatedAt = DateTime.Now,
                };

                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();

                // // Now create and save the intake with the new PatientId
                // var intake = new IntakeForm
                // {
                //     PatientId = patient.PatientId,
                //     DateOfReferral = model.DateOfReferral,
                //     ReferredBy = model.ReferredBy ?? string.Empty,
                //     Affiliation = model.Affiliation,
                //     PhoneNumber = model.ReferredByPhoneNumber,
                //     PresentingComplaint = model.PresentingComplaint,
                //     IntakeStatus = IntakeStatus.Pending.ToString(),
                //     CreatedAt = DateTime.Now
                // };

                // await _context.IntakeForms.AddAsync(intake ?? new IntakeForm());
                // await _context.SaveChangesAsync();
    
                // Save physician-patient relationship if selected
                if (model.ClinicalStaff > 0)
                {
                    var clinicalStaffPatient = new ClinicalStaffPatient
                    {
                        PatientId = patient.PatientId,
                        ClinicalStaffId = model.ClinicalStaff
                    };

                    _context.ClinicalStaffPatients.Add(clinicalStaffPatient);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Intake");
            }   
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving patient: {ex.Message}");
                TempData["Error"] = "There was an error saving the patient.";

                var physicians = await _context.ClinicalStaffs
                    .Where(p => p.Position == "Physician")
                    .Select(p => new SelectListItem
                    {
                        Value = p.ClinicalStaffID.ToString(),
                        Text = $"{p.Firstname} {p.Lastname}"
                    })
                    .ToListAsync();

                ViewBag.Physicians = new SelectList(physicians, "Value", "Text", model.ClinicalStaff);
                return View(model);
            }
        }

        //Action to proceed Profile
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            //Query staff Information
            var patient = await _context.Patients
                        .Include(p => p.ClinicalStaffPatients)
                            .ThenInclude(csp => csp.ClinicalStaff)
                        .FirstOrDefaultAsync(p => p.PatientId == id); // or however you get the patient


            //Check if staff is not null
            if (patient == null)
            {
                TempData["Error"] = "Staff not found";
            }

            // Build the view model
            var viewModel = new ClinicalStaffProfileViewModel
            {
                // Staffs: all staff assigned to this patient
                Staffs = patient.ClinicalStaffPatients
                        .Select(csp => csp.ClinicalStaff)
                        .Distinct()
                        .ToList(),

                // Patients: just one patient in this case
                Patients = new List<Patient> { patient }
            };

            //Return to View 
            return View(viewModel);
        }

        public IActionResult Edit(int Id)
        {

            return View();
        }

        public IActionResult IntakeForm()
        {
            return View();
        }

        public async Task<IActionResult> PatientMasterList()
        {
            ViewBag.TotalPatientCount = await _context.Patients.CountAsync();
            var patients = await _context.Patients
                .Include(p => p.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(patients);
        }
    }
}
