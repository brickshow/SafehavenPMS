using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    [Authorize]
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

            // Restrict results to patients assigned to the logged-in clinical staff (unless Admin)
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
               var appUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                if (appUser != null && !string.Equals(appUser.Role ?? string.Empty, "Admin", StringComparison.OrdinalIgnoreCase))
               {
                    // If User has a ClinicalStaffID, filter by that
                    if (appUser.ClinicalStaffID.HasValue)
                    {
                        var staffId = appUser.ClinicalStaffID.Value;
                        query = query.Where(p => p.ClinicalStaffPatients.Any(csp => csp.ClinicalStaffId == staffId));
                    }
                    else
                    {
                        // Fallback: try to resolve ClinicalStaff by user email and filter by that staff
                        if (!string.IsNullOrWhiteSpace(appUser.Email))
                        {
                            var cs = await _context.ClinicalStaffs
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Email.ToLower() == appUser.Email.Trim().ToLower());
                            if (cs != null)
                            {
                                query = query.Where(p => p.ClinicalStaffPatients.Any(csp => csp.ClinicalStaffId == cs.ClinicalStaffID));
                            }
                            else
                            {
                                // not linked to a clinical staff � do not expose patients
                                query = query.Where(p => false);
                            }
                        }
                        else
                        {
                            query = query.Where(p => false);
                        }
                    }
                }
            }

            // Counts for each status (use the filtered query so counts reflect what the user can see)
            ViewBag.TotalPatientCount = await query.CountAsync(p => p.PatientStatus == PatientStatusEnum.InTreatment.ToString() 
                                                                || p.PatientStatus == PatientStatusEnum.Admitted.ToString());


            // Pass current filters/sorting to view
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // ?? Apply search filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery));
            }

            // ? Apply status filter
            // If a status parameter is provided (or "All"), use it. Otherwise default to showing
            // patients who are Admitted or InTreatment (if that enum exists).
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.PatientStatus == status);
            }
            else
            {
                var admittedStatus = PatientStatusEnum.Admitted.ToString();
                // use InTreatment enum name if present; otherwise fall back to literal string
                string inTreatmentStatus = PatientStatusEnum.InTreatment.ToString();

                query = query.Where(p => p.PatientStatus == admittedStatus || p.PatientStatus == inTreatmentStatus);
                // Leave ViewBag.Status as-is (null/empty) so the UI still shows "All Statuses"
            }

            // ?? Apply sorting
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

            // ?? Pagination
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

        [HttpGet("Patient/AddNewPatient")]
        public async Task<IActionResult> AddNewPatient(string? searchPatientId)
        {
            var model = new AddNewPatientViewModel();

            if (!string.IsNullOrEmpty(searchPatientId))
            {
                // Validate Patient ID format (e.g., PAT-0000001)
                if (!System.Text.RegularExpressions.Regex.IsMatch(searchPatientId, @"^PAT-\d{7}$"))
                {
                    ViewBag.SearchPatientError = "Invalid Patient ID format.";
                    ViewBag.SearchedPatientId = searchPatientId;
                    return View(model);
                }

                // Only search for discharged patients
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientRefId == searchPatientId 
                        && p.PatientStatus == Enum.PatientStatusEnum.Discharged.ToString());

                if (patient != null)
                {
                    // Populate model with patient data
                    model.Firstname = patient.Firstname;
                    model.MiddleName = patient.MiddleName;
                    model.Lastname = patient.Lastname;
                    model.DateOfBirth = patient.DateOfBirth;
                    model.ContactNumber = patient.PhoneNumber;
                    model.Sex = patient.Sex;
                    model.Occupation = patient.Occupation;
                    model.Education = patient.Education;
                    model.Religion = patient.Religion;
                    model.MaritalStatus = patient.MaritalStatus;
                    // Parse address if needed

                    ViewBag.SearchedPatientId = searchPatientId;
                    ViewBag.LockIdentityFields = true;
                    ViewBag.SearchPatientError = "This patient is currently discharged. You may reactivate the record or create a new one.";
                    ViewBag.ShowPatientDischargedModal = true;
                }
                else
                {
                    ViewBag.SearchPatientError = "Discharged patient not found.";
                    ViewBag.SearchedPatientId = searchPatientId;
                    ViewBag.ShowPatientNotFoundModal = true;
                }
            }

            return View(model);
        }

        // Post action for adding a new patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewPatient(AddNewPatientViewModel model, string? DuplicateAction)
        {
            ModelState.Remove("PhotoUrl");
            ModelState.Remove("DuplicateAction"); // Ensure not required

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
                    TempData["Error"] = $"Failed to upload profile image: {ex.Message}";
                    return View(model);
                }
            }

            try
            {
                // Generate unique PatientRefId in the format PAT-0000001
                int lastId = 0;
                var lastPatient = await _context.Patients
                    .OrderByDescending(p => p.PatientId)
                    .FirstOrDefaultAsync();
                if (lastPatient != null && !string.IsNullOrEmpty(lastPatient.PatientRefId))
                {
                    var parts = lastPatient.PatientRefId.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int parsedId))
                    {
                        lastId = parsedId;
                    }
                }
                string newPatientRefId = $"PAT-{(lastId + 1).ToString("D7")}";

                // Check for existing patient
                var existingPatient = await _context.Patients.FirstOrDefaultAsync(p =>
                    p.Firstname.ToLower() == model.Firstname.ToLower() &&
                    p.Lastname.ToLower() == model.Lastname.ToLower() &&
                    p.MiddleName.ToLower() == (model.MiddleName ?? "").ToLower() &&
                    p.DateOfBirth.Date == model.DateOfBirth.Date
                );

                if (existingPatient != null && string.IsNullOrEmpty(DuplicateAction))
                {
                    var status = existingPatient.PatientStatus;

                    if (!string.Equals(status, Enum.PatientStatusEnum.Discharged.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        // Active (not discharged)
                        ViewBag.DuplicatePatient = true;
                        ViewBag.DuplicatePatientInfo = $"{existingPatient.Firstname} {existingPatient.MiddleName} {existingPatient.Lastname} ({existingPatient.DateOfBirth:yyyy-MM-dd})";
                        ViewBag.DuplicatePatientStatus = "Active";
                        ViewBag.DuplicatePatientWarning = "This patient already exists and is currently active in the system. Creating a new record will duplicate patient data and may impact care quality. Do you still want to create a new record?";
                        ViewBag.DuplicatePatientActions = new[] { "Cancel Registration", "Create New Record" };
                        return View(model);
                    }
                    else
                    {
                        // Discharged
                        ViewBag.DuplicatePatient = true;
                        ViewBag.DuplicatePatientInfo = $"{existingPatient.Firstname} {existingPatient.MiddleName} {existingPatient.Lastname} ({existingPatient.DateOfBirth:yyyy-MM-dd})";
                        ViewBag.DuplicatePatientStatus = "Discharged";
                        ViewBag.DuplicatePatientWarning = "This patient already exists but is currently discharged. You can reactivate the existing record to preserve medical history. Creating a new record will duplicate patient data.";
                        ViewBag.DuplicatePatientActions = new[] { "Reactivate", "Create New Record" };
                        return View(model);
                    }
                }

                // Handle duplicate actions
                if (existingPatient != null && DuplicateAction == "Reactivate")
                {
                    existingPatient.PatientStatus = Enum.PatientStatusEnum.NewIntake.ToString();
                    _context.Patients.Update(existingPatient);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Patient record reactivated successfully.";
                    return RedirectToAction("index");
                }
                if (existingPatient != null && (DuplicateAction == "Create New Record" || DuplicateAction == "Create"))
                {
                    // Continue to create a new patient record below
                }
                if (existingPatient != null && DuplicateAction == "Cancel Registration")
                {
                    TempData["Message"] = "Registration cancelled. No changes were made.";
                    return View(model);
                }

                // Create and save patient only
                var patient = new Patient
                {
                    PatientRefId = newPatientRefId,
                    Firstname = model.Firstname,
                    MiddleName = model.MiddleName ?? string.Empty,
                    Lastname = model.Lastname,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.ContactNumber ?? string.Empty,
                    Sex = model.Sex,
                    Occupation = model.Occupation ?? string.Empty,
                    PatientStatus = Enum.PatientStatusEnum.NewIntake.ToString(),
                    Education = model.Education ?? string.Empty,
                    Religion = model.Religion ?? string.Empty,
                    MaritalStatus = model.MaritalStatus,
                    PhotoUrl = tempUrl ?? string.Empty,
                    Address = $"{model.House_Unit}, {model.Street}, {model.Subdivision_Village}, {model.Barangay}, {model.City}, {model.Province}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = GetCurrentUserName()
                };

                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();

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

                TempData["Message"] = "New patient record created.";
                return RedirectToAction("Index", "Intake");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "There was an error saving the patient.";
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
                .Where(p => p.PatientStatus == PatientStatusEnum.InTreatment.ToString() 
                         || p.PatientStatus == PatientStatusEnum.Admitted.ToString())
                .ToListAsync();
            return View(patients);
        }

        private string GetCurrentUserName()
        {
            // Try to get display name first
            var name = User?.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(name)) return name;

            // Fall back to email if name not available
            var email = User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email)) return email;

            // Fall back to basic identity name
            var identityName = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(identityName)) return identityName;

            // Last resort
            return "Unknown";
        }
    }
}

