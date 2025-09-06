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


namespace SafehavenPMS.Controllers
{
    public class IntakeController : Controller
    {
        private readonly SafehavenPMSContext _context;
        public IntakeController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
                   int? page = 1,
                   int? pageSize = 10,
                   string searchQuery = null,
                   string status = null,
                   string sortOrder = null)
        {
            var query = _context.Patients
                .Include(i => i.IntakeForm)
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

            // Apply status filter (default = All)
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.PatientStatus.ToString() == status);
            }

            //Apply sorting
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

            // Project to IntakeViewModel
             var intakeViewModels = patientList
                                    .Where(p => p.PatientStatus == PatientStatusEnum.NewReferral.ToString() ||
                                                p.PatientStatus == PatientStatusEnum.Waitlisted.ToString() ||
                                                p.PatientStatus == PatientStatusEnum.InProgress.ToString())
                                    .Select(p => new SafehavenPMS.ViewModel.IntakeViewModel
                                    {
                                        PatientId = p.PatientId,
                                        FullName = $"{p.Firstname} {p.Lastname}",
                                        ReferredBy = p.IntakeForm?.ReferredBy ?? string.Empty,
                                        ReferredByPhoneNumber = p.IntakeForm?.PhoneNumber ?? string.Empty,
                                        IntakeOfficer = "-", // Populate if you have this info
                                        IntakeDate = p.IntakeForm?.CreatedAt != null ? ((DateTime)p.IntakeForm.CreatedAt).ToString("yyyy-MM-dd") : "-",
                                        CompletedDate = "-", // Populate if you have this info
                                        // SAFE: don't call ToString() on a null IntakeForm or null IntakeStatus
                                        IntakeStatus = p.PatientStatus ?? "-",
                                    }).ToList() ?? new List<SafehavenPMS.ViewModel.IntakeViewModel>();

            //Return Total number of new referral
            var Pending = await _context.Patients
                                    .Where(p => p.PatientStatus == PatientStatusEnum.NewReferral.ToString())
                                    .ToListAsync();

            ViewBag.Pending = Pending.Count();
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
                return NotFound();

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
                ReferredBy = intake.IntakeForm.ReferredBy,
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
                intakeForm.IntakeForm.ReferredBy = string.IsNullOrWhiteSpace(model.ReferredBy) ? intakeForm.IntakeForm.ReferredBy : model.ReferredBy.Trim();
                intakeForm.IntakeForm.PhoneNumber = string.IsNullOrWhiteSpace(model.ReferredByPhoneNumber) ? intakeForm.IntakeForm.PhoneNumber : model.ReferredByPhoneNumber.Trim();
                intakeForm.IntakeForm.PresentingComplaint = model.ReasonForIntake ?? intakeForm.IntakeForm.PresentingComplaint;
                intakeForm.IntakeForm.CreatedAt = DateTime.UtcNow; // Update timestamp
                intakeForm.IntakeForm.Affiliation = model.Affiliation ?? intakeForm.IntakeForm.Affiliation;
                //Butanganan pas uban fields

                // mark as in-progress when details saved
                _context.IntakeForms.Update(intakeForm.IntakeForm);

                // If patient is NewReferral, mark InProgress
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Intake details saved.";
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
                return NotFound();
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
                _context.IntakeForms.Update(intakeForm.IntakeForm);

                //Call helper to update patient status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Family information saved successfully. Added {intakeForm.IntakeForm.FamilyMembers.Count} family members.";
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
                return NotFound();
            }

            try
            {
                // Update presenting problems
                intakeForm.IntakeForm.ProblemPresentation = model.ProblemPresentation;
                _context.IntakeForms.Update(intakeForm.IntakeForm);

                // Update status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Presenting problems saved successfully.";
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
                return NotFound();
            }

            try
            {
                // Update counselor impressions
                intakeForm.IntakeForm.CouncilorImpression = model.CouncilorImpression;
                _context.IntakeForms.Update(intakeForm.IntakeForm);

                // Update status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Counselor impressions saved successfully.";
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
                patient.PatientStatus = PatientStatusEnum.Waitlisted.ToString();
            } 
            if (patient.PatientStatus == PatientStatusEnum.NewReferral.ToString())
            {
                patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
            }
         
            await _context.SaveChangesAsync();

        }

        //Action to Submit Intake form for assessment
        [HttpPost]
        public async Task<IActionResult> SubmitIntakeForm(int PatientId)
        {
            // Logic to submit the intake form for assessment
            var patient = await _context.Patients.FindAsync(PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            // Update the status to submitted
            _context.Patients.Update(patient);

            //update patient status
            await UpdatePatientStatus(patient.PatientId);
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting the intake form.";
                return RedirectToAction("EditIntakeForm", new { id = patient.PatientId });
            }
            return RedirectToAction("Index");
        }
    }
}