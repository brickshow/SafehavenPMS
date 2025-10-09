using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SafehavenPMS.ViewModel;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using SafehavenPMS.Services;


namespace SafehavenPMS.Controllers
{
[Authorize]
    public class IntakeController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly ActivityLogService _activityService; // added
        public IntakeController(SafehavenPMSContext context, ActivityLogService activityService) // modified
        {
            _context = context;
            _activityService = activityService;
        }

        private static string GetPatientFullName(Patient p) => $"{p.Firstname} {p.Lastname}";

        public async Task<IActionResult> Index(
    int? page = 1,
    int? pageSize = 10,
    string searchQuery = null,
    string status = null,
    string sortOrder = null,
    string sortBy = null)
        {
            if (string.IsNullOrEmpty(status))
            {
                status = "All";
            }

            var currentUser = User?.Identity?.Name;

            var query = _context.Patients
                .Include(i => i.IntakeForm)
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .Where(p => p.CreatedBy == currentUser || p.IntakeForm.CreatedBy == currentUser) // Add this line
                .AsQueryable();

            // Rest of your existing role-based restrictions
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                var appUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                if (appUser != null && !string.Equals(appUser.Role ?? string.Empty, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    if (appUser.ClinicalStaffID.HasValue)
                    {
                        var csId = appUser.ClinicalStaffID.Value;
                        query = query.Where(p =>
                            p.ClinicalStaffPatients.Any(csp => csp.ClinicalStaffId == csId) ||
                            p.CreatedBy == currentUser ||
                            p.IntakeForm.CreatedBy == currentUser);
                    }
                    else
                    {
                        // If not admin and not clinical staff, only show own created records
                        query = query.Where(p => p.CreatedBy == currentUser || p.IntakeForm.CreatedBy == currentUser);
                    }
                }
            }

            // Modify the status counts to reflect only the filtered patients
            ViewBag.TotalPatientCount = await query.CountAsync();
            ViewBag.WaitlistedCount = await query.CountAsync(p => p.PatientStatus == PatientStatusEnum.Waitlisted.ToString());
            ViewBag.PendingAssessmentCount = await query.CountAsync(p => p.PatientStatus == PatientStatusEnum.PendingAssessment.ToString());
            ViewBag.PendingApprovalCount = await query.CountAsync(p => p.PatientStatus == PatientStatusEnum.PendingApproval.ToString());

            // Pass current filters/sorting to view
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;
            ViewBag.SortBy = string.IsNullOrEmpty(sortBy) ? "" : sortBy;

            // 🔎 Apply search filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery));
            }

            // Apply status filter (only when a specific status other than "All" is requested)
            // Normalize incoming status (trim + case-insensitive) to avoid mismatches
            var normalizedStatus = status?.Trim();
            if (!string.IsNullOrEmpty(normalizedStatus) && !normalizedStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var nsLower = normalizedStatus.ToLower();

                // Special case: "Completed" = any status except NewIntake, InProgress, Discharged
                if (nsLower == "completed")
                {
                    var newIntake = PatientStatusEnum.NewIntake.ToString().ToLower();
                    var inProgress = PatientStatusEnum.InProgress.ToString().ToLower();
                    var discharged = PatientStatusEnum.Discharged.ToString().ToLower();

                    query = query.Where(p =>
                        p.PatientStatus != null &&
                        p.PatientStatus.ToLower() != newIntake &&
                        p.PatientStatus.ToLower() != inProgress &&
                        p.PatientStatus.ToLower() != discharged);
                }
                else
                {
                    // Compare case-insensitively and guard against null PatientStatus
                    query = query.Where(p => p.PatientStatus != null && p.PatientStatus.ToLower() == nsLower);
                }
            }
            // NOTE: If "Completed" is a derived state (e.g. based on IntakeForm fields) you must replace the predicate
            // with the appropriate check, e.g.:
            // if (nsLower == "completed") query = query.Where(p => p.IntakeForm != null && p.IntakeForm.SomeCompletedFlag == true);

            // Apply sorting based on sortBy + sortOrder
            // sortBy: "Name" | "DateofIntake" | empty = default (CreatedAt desc)
            if (string.IsNullOrEmpty(sortBy))
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
            {
                query = string.Equals(sortOrder, "ascending", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                    : query.OrderByDescending(p => p.Firstname).ThenByDescending(p => p.Lastname);
            }
            else if (string.Equals(sortBy, "DateofIntake", StringComparison.OrdinalIgnoreCase))
            {
                // Use IntakeForm.CreatedAt when available, fallback to Patient.CreatedAt
                query = string.Equals(sortOrder, "ascending", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderBy(p => p.IntakeForm != null ? p.IntakeForm.CreatedAt : p.CreatedAt)
                    : query.OrderByDescending(p => p.IntakeForm != null ? p.IntakeForm.CreatedAt : p.CreatedAt);
            }
            else
            {
                // fallback default
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            // Update the view model projection to include creator information
            var intakeViewModels = patientList
                .Select(p => new SafehavenPMS.ViewModel.IntakeViewModel
                {
                    PatientId = p.PatientId,
                    FullName = $"{p.Firstname} {p.Lastname}",
                    ReferredBy = p.IntakeForm?.AccompaniedBy ?? string.Empty,
                    ReferredByPhoneNumber = p.IntakeForm?.PhoneNumber ?? string.Empty,
                    CreatedBy = p.CreatedBy ?? p.IntakeForm?.CreatedBy ?? "System",
                    IntakeDate = p.IntakeForm?.CreatedAt != null ? ((DateTime)p.IntakeForm.CreatedAt).ToString("yyyy-MM-dd") : "-",
                    CompletedDate = p.CreatedAt != null ? ((DateTime)p.CreatedAt).ToString("MMM dd, yyyy") : "-",
                    IntakeStatus = p.PatientStatus ?? "-",
                }).ToList() ?? new List<SafehavenPMS.ViewModel.IntakeViewModel>();

            ViewBag.Pending = await query
                .CountAsync(p => p.PatientStatus == PatientStatusEnum.NewIntake.ToString());

            return View(intakeViewModels);
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
        public async Task<IActionResult> EditIntakeForm(int id)
        {
            var intake = await _context.Patients
                .Include(p => p.IntakeForm)
                .ThenInclude(i => i.FamilyMembers)
                .FirstOrDefaultAsync(i => i.PatientId == id);

            if (intake == null)
                return View();  

            // Calculate age from DoB
            string age = "-";
            if (intake?.DateOfBirth != null)
            {
                var today = DateTime.Today;
                var dob = intake.DateOfBirth;
                age = (today.Year - dob.Year - (dob.Date > today.AddYears(-(today.Year - dob.Year)) ? 1 : 0)).ToString();
            }

            var vm = new IntakeViewModel
            {
                PatientId = intake.PatientId,
                FullName = $"{intake.Firstname} {intake.Lastname}",
                Age = age,
                Sex = intake.Sex ?? "-",
                Address = intake.Address ?? "-",
                ReferredBy = intake.IntakeForm.AccompaniedBy,
                Affiliation = intake.IntakeForm.Affiliation,
                ReferredByPhoneNumber = intake.IntakeForm.PhoneNumber,
                IntakeOfficer = "-",
                IntakeDate = intake?.CreatedAt != null ? ((DateTime)intake.CreatedAt).ToString("yyyy-MM-dd") : "-",
                Occupation = intake?.Occupation ?? "-",
                ReasonForIntake = intake?.IntakeForm.PresentingComplaint,
                CouncilorImpression = intake?.IntakeForm.CouncilorImpression,
                ProblemPresentation = intake?.IntakeForm.ProblemPresentation,
                OtherFamilyDetails = intake?.IntakeForm.OtherFamilyDetails,


                // Add this: Load existing family members
                FamilyMembers = intake?.IntakeForm.FamilyMembers?.Select(fm => new FamilyMemberVm
                {
                    Name = fm.Name,
                    Age = fm.Age,
                    Relationship = fm.Relationship,
                    Comments = fm.Comments,

                }).ToList() ?? new List<FamilyMemberVm>(),
            };

            return View(vm);
        }

        // Save Details tab (DetailsTab.cshtml) - updates IntakeForm fields
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDetails([FromForm] IntakeViewModel model)
        {
            // Validate request
            if (model == null || model.PatientId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction("Index");
            }

            // ModelState validation -> send errors via TempData and redirect back to edit page
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            var intakeForm = await _context.Patients
                .Include(i => i.IntakeForm)
                .FirstOrDefaultAsync(i => i.PatientId == model.PatientId);

            if (intakeForm == null)
            {
                TempData["ErrorMessage"] = "Intake form not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Update fields from the Details tab
                intakeForm.IntakeForm.AccompaniedBy = string.IsNullOrWhiteSpace(model.ReferredBy) ? intakeForm.IntakeForm.AccompaniedBy : model.ReferredBy.Trim();
                intakeForm.IntakeForm.PhoneNumber = string.IsNullOrWhiteSpace(model.ReferredByPhoneNumber) ? intakeForm.IntakeForm.PhoneNumber : model.ReferredByPhoneNumber.Trim();
                intakeForm.IntakeForm.PresentingComplaint = model.ReasonForIntake ?? intakeForm.IntakeForm.PresentingComplaint;
                intakeForm.IntakeForm.CreatedAt = DateTime.UtcNow; // Update timestamp
                intakeForm.IntakeForm.Affiliation = model.Affiliation ?? intakeForm.IntakeForm.Affiliation;
                //Butanganan pas uban fields

                // Set Intake Officer to the currently authenticated user (if the entity has the property)
                var officerName = User?.Identity?.Name ?? "";
                if (!string.IsNullOrEmpty(officerName))
                {
                    var intakeEntity = intakeForm.IntakeForm;
                    var prop = intakeEntity?.GetType().GetProperty("IntakeOfficer");
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(intakeEntity, officerName);
                    }

                    // Use CreatedBy attribute on Patient and IntakeForm if available
                    intakeForm.IntakeForm.CreatedBy = officerName;
                    var createdByProp = intakeEntity?.GetType().GetProperty("CreatedBy");
                    if (createdByProp != null && createdByProp.CanWrite)
                    {
                        createdByProp.SetValue(intakeEntity, officerName);
                    }
                }

                // mark as in-progress when details saved
                _context.IntakeForms.Update(intakeForm.IntakeForm);
                
                // If patient is NewReferral, mark InProgress
                await UpdatePatientStatus(intakeForm.PatientId);
                
                await _context.SaveChangesAsync();
                
                var fullName = GetPatientFullName(intakeForm);
                TempData["SuccessMessage"] = "Intake details saved.";
                await _activityService.LogAsync(
                    User?.Identity?.Name ?? "System",
                    "Updated intake details",
                    $"Updated basic intake details for patient {fullName}",
                    "Intake",
                    "Info",
                    intakeForm.PatientId);
            }
            catch (Exception ex)
            {
                // Log if needed and surface error via TempData
                TempData["ErrorMessage"] = "An error occurred while saving intake details.";
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            // Redirect back to edit view
            return RedirectToAction("EditIntakeForm", new { id = intakeForm.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFamilyData([FromForm] IntakeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return to EditIntakeForm with the current model
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            var intakeForm = await _context.Patients
                .Include(p => p.IntakeForm)
                .ThenInclude(i => i.FamilyMembers)
                .FirstOrDefaultAsync(i => i.PatientId == model.PatientId);

            if (intakeForm == null)
            {
                return View();
            }

            try
            {
                // Remove existing family members
                _context.FamilyMembers.RemoveRange(intakeForm.IntakeForm.FamilyMembers);

                // Add family members from the model
                if (model.FamilyMembers != null)
                {
                    foreach (var familyMember in model.FamilyMembers.Where(fm => !string.IsNullOrWhiteSpace(fm.Name)))
                    {
                        intakeForm.IntakeForm.FamilyMembers.Add(new FamilyMember
                        {
                            Name = familyMember.Name,
                            Age = familyMember.Age,
                            Relationship = familyMember.Relationship,
                            Comments = familyMember.Comments,
                            IntakeFormId = intakeForm.IntakeForm.IntakeFormsId
                        });
                    }
                }

                // Update other family details and intake form status
                intakeForm.IntakeForm.OtherFamilyDetails = model.OtherFamilyDetails;

                // Set Intake Officer to current user (if the entity supports it)
                var officerName2 = User?.Identity?.Name ?? "";
                if (!string.IsNullOrEmpty(officerName2))
                {
                    var intakeEntity2 = intakeForm.IntakeForm;
                    var prop2 = intakeEntity2?.GetType().GetProperty("IntakeOfficer");
                    if (prop2 != null && prop2.CanWrite)
                    {
                        prop2.SetValue(intakeEntity2, officerName2);
                    }

                    // Use CreatedBy attribute
                    intakeForm.IntakeForm.CreatedBy = officerName2;
                    var createdByProp2 = intakeEntity2?.GetType().GetProperty("CreatedBy");
                    if (createdByProp2 != null && createdByProp2.CanWrite)
                    {
                        createdByProp2.SetValue(intakeEntity2, officerName2);
                    }
                }

                _context.IntakeForms.Update(intakeForm.IntakeForm);

                //Call helper to update patient status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();
                
                var fullName = GetPatientFullName(intakeForm);
                TempData["SuccessMessage"] = $"Family information saved successfully. Added {intakeForm.IntakeForm.FamilyMembers.Count} family members.";
                await _activityService.LogAsync(
                    User?.Identity?.Name ?? "System",
                    "Updated family data",
                    $"Saved {intakeForm.IntakeForm.FamilyMembers.Count} family members for patient {fullName}",
                    "Intake",
                    "Info",
                    intakeForm.PatientId);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "An error occurred while saving family information.";
                return RedirectToAction("EditIntakeForm", new { id = model.PatientId });
            }

            return RedirectToAction("EditIntakeForm", new { id = intakeForm.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProblems(IntakeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            var intakeForm = await _context.Patients
                .Include(p => p.IntakeForm)
                .FirstOrDefaultAsync(i => i.PatientId == model.PatientId);

            if (intakeForm == null)
            {
                return View();
            }

            try
            {
                // Update presenting problems
                intakeForm.IntakeForm.ProblemPresentation = model.ProblemPresentation;

                // Set Intake Officer to current user (if possible)
                var officerName3 = User?.Identity?.Name ?? "";
                if (!string.IsNullOrEmpty(officerName3))
                {
                    var intakeEntity3 = intakeForm.IntakeForm;
                    var prop3 = intakeEntity3?.GetType().GetProperty("IntakeOfficer");
                    if (prop3 != null && prop3.CanWrite)
                    {
                        prop3.SetValue(intakeEntity3, officerName3);
                    }

                    // Use CreatedBy attribute
                    intakeForm.IntakeForm.CreatedBy = officerName3;
                    var createdByProp3 = intakeEntity3?.GetType().GetProperty("CreatedBy");
                    if (createdByProp3 != null && createdByProp3.CanWrite)
                    {
                        createdByProp3.SetValue(intakeEntity3, officerName3);
                    }
                }
                
                _context.IntakeForms.Update(intakeForm.IntakeForm);

                // Update status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();
                var fullName = GetPatientFullName(intakeForm);
                TempData["SuccessMessage"] = "Presenting problems saved successfully.";
                await _activityService.LogAsync(
                    User?.Identity?.Name ?? "System",
                    "Updated presenting problems",
                    $"Presenting problems updated for patient {fullName}",
                    "Intake",
                    "Info",
                    intakeForm.PatientId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving presenting problems.";
                return RedirectToAction("EditIntakeForm", new { id = model.PatientId});
            }

            return RedirectToAction("EditIntakeForm", new { id = intakeForm.PatientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImpressions(IntakeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            var intakeForm = await _context.Patients
                .Include(p => p.IntakeForm)
                .FirstOrDefaultAsync(i => i.PatientId == model.PatientId);

            //TODO RETURN TEMDATA
            if (intakeForm == null)
            {
                return View();
            }

            try
            {
                // Update counselor impressions
                intakeForm.IntakeForm.CouncilorImpression = model.CouncilorImpression;

                // Set Intake Officer to current user (if possible)
                var officerName4 = User?.Identity?.Name ?? "";
                if (!string.IsNullOrEmpty(officerName4))
                {
                    var intakeEntity4 = intakeForm.IntakeForm;
                    var prop4 = intakeEntity4?.GetType().GetProperty("IntakeOfficer");
                    if (prop4 != null && prop4.CanWrite)
                    {
                        prop4.SetValue(intakeEntity4, officerName4);
                    }

                    // Use CreatedBy attribute
                    intakeForm.IntakeForm.CreatedBy = officerName4;
                    var createdByProp4 = intakeEntity4?.GetType().GetProperty("CreatedBy");
                    if (createdByProp4 != null && createdByProp4.CanWrite)
                    {
                        createdByProp4.SetValue(intakeEntity4, officerName4);
                    }
                }
                
                _context.IntakeForms.Update(intakeForm.IntakeForm);

                // Update status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();
                var fullName = GetPatientFullName(intakeForm);
                TempData["SuccessMessage"] = "Counselor impressions saved successfully.";
                await _activityService.LogAsync(
                    User?.Identity?.Name ?? "System",
                    "Updated counselor impressions",
                    $"Counselor impressions updated for patient {fullName}",
                    "Intake",
                    "Info",
                    intakeForm.PatientId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving counselor impressions.";
                return RedirectToAction("EditIntakeForm", new { id = model.PatientId });
            }

            return RedirectToAction("EditIntakeForm", new { id = intakeForm.PatientId });
        }

        //Helper method to update patient status
        private async Task UpdatePatientStatus(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(i => i.PatientId == id);

            if (patient == null)
                return;

            // All sections completed
      
            if (patient.PatientStatus == PatientStatusEnum.InProgress.ToString())
            {
                patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
            } 
            if (patient.PatientStatus == PatientStatusEnum.NewIntake.ToString())
            {
                patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
            }   
         
            await _context.SaveChangesAsync();
            var fullName = GetPatientFullName(patient);
            await _activityService.LogAsync(
                User?.Identity?.Name ?? "System",
                "Updated patient status",
                $"Patient {fullName} status now {patient.PatientStatus}",
                "Status",
                "Info",
                patient.PatientId);
        }

        //Action to Submit Intake form for assessment
        [HttpPost]
        public async Task<IActionResult> SubmitIntakeForm(int PatientId)
        {
            // Logic to submit the intake form for assessment
            var patient = await _context.Patients.FindAsync(PatientId);
            if (patient == null)
            {
                return View();
            }

            // set CreatedBy on patient when submitting
            var submitter = User?.Identity?.Name ?? "";
            if (!string.IsNullOrEmpty(submitter))
            {
                patient.IntakeForm.CreatedBy = submitter;
            }

            // Update the status to submitted
            _context.Patients.Update(patient);

            //update patient status
            patient.PatientStatus = PatientStatusEnum.Waitlisted.ToString(); 
            await _context.SaveChangesAsync();

            try
            {
                //pre populate scheduling tables
                var scheduling = new NewAppointment
                {
                    PatientId = patient.PatientId,
                    Type = "Initial Assessment",
                    Status = SchedulingStatus.Pending.ToString()
                };
                await _context.NewAppointments.AddAsync(scheduling);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Intake form submitted for assessment successfully.";
                var fullName = GetPatientFullName(patient);
                await _activityService.LogAsync(
                    User?.Identity?.Name ?? "System",
                    "Submitted intake form",
                    $"Intake form submitted; patient {fullName} status set to Waitlisted",
                    "Intake",
                    "Info",
                    patient.PatientId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting the intake form.";
                return RedirectToAction("EditIntakeForm", new { id = patient.PatientId });
            }
            return RedirectToAction("Index");
        }

        // Keep existing view form working: redirect SortBy -> Index with params
        [HttpGet]
        public IActionResult SortBy(string sortBy, string sortOrder, string searchQuery, int? pageSize, string status)
        {
            return RedirectToAction("Index", new
            {
                page = 1,
                pageSize = pageSize ?? 10,
                searchQuery,
                status,
                sortOrder,
                sortBy
            });
        }
    }
}
