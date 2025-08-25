using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class AdmissionController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public AdmissionController(SafehavenPMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchQuery, string status, string sortOrder, int page = 1, int pageSize = 10)
        {
            var query = _context.Admissions.Include(a => a.Patient).AsQueryable();

            // --- SEARCH ---
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(a =>
                    a.Patient.Firstname.Contains(searchQuery) ||
                    a.Patient.Lastname.Contains(searchQuery));
            }

            // --- FILTER BY STATUS ---
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.status == status);
            }

            // --- SORT ---
            query = sortOrder == "descending"
                ? query.OrderByDescending(a => a.AdmissionDate)
                : query.OrderBy(a => a.AdmissionDate);

            // --- TOTAL COUNT for pagination ---
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // --- PAGINATION ---
            var admissions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalPatientCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            // Map to ViewModel (keep your existing fields)
            var model = admissions.Select(a => new AdmitPatientViewModel
            {
                AdmissionId = a.AdmissionId,
                PatientId = a.PatientId,
                FullName = $"{a.Patient.Firstname} {a.Patient.Lastname}",
                AdmissionDate = a.AdmissionDate,
                Status = a.status,
                EndDate = a.EndDate,
                EndedBy = a.Endedby
            }).ToList();

            return View(model); // your current view
        }

        // Step 1: Search for patient with pending review
        [HttpPost]
        public async Task<IActionResult> SearchPatient(int searchQuery)
        {
            // searchQuery is PatientId from dropdown
            var patient = await _context.Patients
                .Include(p => p.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .FirstOrDefaultAsync(p => p.PatientId == searchQuery &&
                                          p.PatientStatus == Enum.PatientStatusEnum.PendingReview.ToString());

            if (patient == null)
                return View("AdmitPatient", new AdmitPatientViewModel());

            var physician = patient.ClinicalStaffPatients
                .Select(csp => csp.ClinicalStaff)
                .FirstOrDefault();

            var vm = new AdmitPatientViewModel
            {
                PatientId = patient.PatientId,
                FullName = $"{patient.Firstname} {patient.MiddleName} {patient.Lastname}".Trim(),
                Sex = patient.Sex,
                DOB = patient.DateOfBirth,
                EducationalAttainment = patient.Education,
                Occupation = patient.Occupation,
                Religion = patient.Religion,
                PhoneNumber = patient.PhoneNumber,
                PhysicianId = physician?.ClinicalStaffID,
                PhysicianName = physician != null ? $"{physician.Firstname} {physician.Lastname}" : ""
            };

            await PopulateClinicalStaffDropdowns();
            return View("AdmitPatient", vm);
        }


        public async Task<IActionResult> AdmitPatient()
        {
            // Fetch all patients with PendingReview status
            var patients = await _context.Patients
                .Where(p => p.PatientStatus == Enum.PatientStatusEnum.PendingReview.ToString())
                .ToListAsync();

            // Populate ViewBag for dropdown
            ViewBag.PatientList = new SelectList(
                patients.Select(p => new
                {
                    p.PatientId,
                    FullName = $"{p.Firstname} {p.Lastname}"
                }),
                "PatientId",
                "FullName"
            );

            // Always provide a non-null model
            return View(new AdmitPatientViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> AdmitPatient(AdmitPatientViewModel model)
        {
            // 1. Validate model
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    var key = state.Key;
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }
                // Repopulate dropdowns for clinical staff if needed
                await PopulateClinicalStaffDropdowns();
                return View(model);
            }

            // 2. Map ViewModel to Admission model
            // Proceed to save admission
            var admission = new Admission
            {
                PatientId = model.PatientId,
                PhysicianId = model.PhysicianId,
                PsychiatristId = model.PsychiatristId,
                PsychologistId = model.PsychologistId,
                PsychometricianId = model.PsychometricianId,
                SocialWorkerId = model.SocialWorkerId,
                RecoveryCoachId = model.RecoveryCoachId,
                FamilyName = model.FamilyName,
                FamilyRelationship = model.FamilyRelationship,
                FamilyPhone = model.FamilyPhone,
                FamilyEmail = model.FamilyEmail,
                ActivatePortal = model.ActivatePortal,
                AdmissionDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = "System", // Replace with logged-in user
                status = Enum.AdmissionStatus.Active.ToString()
            };

            var patient = await _context.Patients.FirstOrDefaultAsync(s => s.PatientId == model.PatientId);

            if (patient == null)
            {
                ModelState.AddModelError("", "Patient not found.");
                await PopulateClinicalStaffDropdowns();
                return View(model);
            }


            //Update patient Status into active
            patient.PatientStatus = PatientStatusEnum.Active.ToString();

            // 3. Save to database
            _context.Patients.Update(patient);
            _context.Admissions.Add(admission);
            await _context.SaveChangesAsync();

            // 4. Redirect or return success message
            TempData["SuccessMessage"] = $"Patient {model.FullName} admitted successfully!";
            return RedirectToAction("Index"); // or wherever you want to go
        }

        // Helper to populate staff dropdowns in case of validation failure
        private async Task PopulateClinicalStaffDropdowns()
        {
            var allStaff = await _context.ClinicalStaffs.ToListAsync();

            ViewBag.Physicians = allStaff
               .Where(s => s.Position == "Physician")
               .Select(s => new SelectListItem
               {
                   Value = s.ClinicalStaffID.ToString(),
                   Text = s.Firstname + " " + s.Lastname
               }).ToList();

            ViewBag.Psychiatrists = allStaff
                .Where(s => s.Position == "Psychiatrist")
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                }).ToList();

            ViewBag.Psychologists = allStaff
                .Where(s => s.Position == "Psychologist")
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                }).ToList();

            ViewBag.Psychometricians = allStaff
                .Where(s => s.Position == "Psychometrician")
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                }).ToList();

            ViewBag.SocialWorkers = allStaff
                .Where(s => s.Position == "Social Worker")
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                }).ToList();

            ViewBag.RecoveryCoaches = allStaff
                .Where(s => s.Position == "Recovery Coach")
                .Select(s => new SelectListItem
                {
                    Value = s.ClinicalStaffID.ToString(),
                    Text = s.Firstname + " " + s.Lastname
                }).ToList();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var admission = await _context.Admissions
                .Include(a => a.Patient) // include patient for FullName, DOB, etc.
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null)
            {
                return NotFound();
            }

            var vm = new AdmitPatientViewModel
            {
                AdmissionId = admission.AdmissionId,
                PatientId = admission.PatientId,
                FullName = $"{admission.Patient.Firstname} {admission.Patient.Lastname}",
                DOB = admission.Patient.DateOfBirth,
                EducationalAttainment = admission.Patient.Education,
                Occupation = admission.Patient.Occupation,
                Religion = admission.Patient.Religion,
                PhoneNumber = admission.Patient.PhoneNumber,

                // Staff assignments
                PhysicianId = admission.PhysicianId,
                PsychiatristId = admission.PsychiatristId,
                PsychologistId = admission.PsychologistId,
                PsychometricianId = admission.PsychometricianId,
                SocialWorkerId = admission.SocialWorkerId,
                RecoveryCoachId = admission.RecoveryCoachId,

                // Family Info
                FamilyName = admission.FamilyName,
                FamilyRelationship = admission.FamilyRelationship,
                FamilyPhone = admission.FamilyPhone,
                FamilyEmail = admission.FamilyEmail,
                ActivatePortal = admission.ActivatePortal
            };

            // populate dropdowns (just like AdmitPatient)
            await PopulateClinicalStaffDropdowns();

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdmitPatientViewModel model)
        {
            // Check if model binding/validation passed (required fields, etc.)
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns again because ViewBags get cleared on postback
                await PopulateClinicalStaffDropdowns();

                // Return the same view with validation errors and filled model
                return View(model);
            }

            // Fetch the admission record from database using AdmissionId
            var admission = await _context.Admissions
                .Include(a => a.Patient) // include Patient entity so we can update patient info if needed
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            // If no record found, return 404 Not Found
            if (admission == null)
            {
                return NotFound();
            }

            // ===== Update Admission fields with values from the form =====
            admission.PhysicianId = model.PhysicianId;                 // Update Physician
            admission.PsychiatristId = model.PsychiatristId;           // Update Psychiatrist
            admission.PsychologistId = model.PsychologistId;           // Update Psychologist
            admission.PsychometricianId = model.PsychometricianId;     // Update Psychometrician
            admission.SocialWorkerId = model.SocialWorkerId;           // Update Social Worker
            admission.RecoveryCoachId = model.RecoveryCoachId;         // Update Recovery Coach

            // Family / payer information
            admission.FamilyName = model.FamilyName;                   // Update Family Contact Name
            admission.FamilyRelationship = model.FamilyRelationship;   // Update Relationship to Patient
            admission.FamilyPhone = model.FamilyPhone;                 // Update Contact Phone
            admission.FamilyEmail = model.FamilyEmail;                 // Update Contact Email
            admission.ActivatePortal = model.ActivatePortal;           // Update whether Family Portal is active

            // ===== OPTIONAL: Update Patient fields if you want them editable =====
            // admission.Patient.Occupation = model.Occupation;
            // admission.Patient.Religion = model.Religion;
            // admission.Patient.PhoneNumber = model.PhoneNumber;

            try
            {
                // Mark the admission as modified in EF and save changes
                _context.Update(admission);
                await _context.SaveChangesAsync();

                // Show success message after saving
                TempData["SuccessMessage"] = "Admission updated successfully!";
            }
            catch (DbUpdateException ex)
            {
                // Handle database update errors (e.g., SQL constraint violation)
                ModelState.AddModelError("", "Unable to save changes. Please try again.");

                // Log error for debugging
                Console.WriteLine(ex);

                // Repopulate dropdowns again so view doesn’t break
                await PopulateClinicalStaffDropdowns();

                // Return view with error
                return View(model);
            }

            // Redirect back to the list of admissions (Index) after success
            return RedirectToAction("Index");
        }

       
    }
}
