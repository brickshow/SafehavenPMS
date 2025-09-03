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
            var intakeViewModels = patientList.Select(p => new SafehavenPMS.ViewModel.IntakeViewModel
            {
                IntakeId = p.IntakeForm?.IntakeFormsId ?? 0,
                FullName = $"{p.Firstname} {p.Lastname}",
                ReferredBy = p.IntakeForm?.ReferredBy ?? "-",
                ReferredByPhoneNumber = p.PhoneNumber,
                IntakeOfficer = "-", // Populate if you have this info
                IntakeDate = p.IntakeForm?.CreatedAt ?? p.CreatedAt,
                CompletedDate = "-", // Populate if you have this info
                IntakeStatus = p.IntakeForm?.IntakeStatus.ToString() ?? "-"
            }).ToList();

            //Return Total number of new referral
            var Pending = await _context.IntakeForms
                                    .Where(p => p.IntakeStatus == Enum.IntakeStatus.Pending.ToString())
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
            var intake = await _context.IntakeForms
                .Include(p => p.Patient)
                .Include(i => i.FamilyMembers) // Add this line
                .FirstOrDefaultAsync(i => i.IntakeFormsId == id);

            if (intake == null)
                return NotFound();

            // Calculate age from DoB
            string age = "-";
            if (intake.Patient?.DateOfBirth != null)
            {
                var today = DateTime.Today;
                var dob = intake.Patient.DateOfBirth;
                age = (today.Year - dob.Year - (dob.Date > today.AddYears(-(today.Year - dob.Year)) ? 1 : 0)).ToString();
            }

            var vm = new IntakeViewModel
            {
                IntakeId = intake.IntakeFormsId,
                FullName = $"{intake.Patient?.Firstname} {intake.Patient?.Lastname}",
                Age = age,
                Sex = intake.Patient?.Sex ?? "-",
                Address = intake.Patient?.Address ?? "-",
                ReferredBy = intake.ReferredBy,
                ReferredByPhoneNumber = intake.Patient?.PhoneNumber,
                IntakeOfficer = "-",
                IntakeDate = intake.CreatedAt,
                DateOfReferral = intake.DateOfReferral,
                Occupation = intake.Patient?.Occupation ?? "-",
                ReasonForIntake = intake.PresentingComplaint,
                IntakeStatus = intake.IntakeStatus?.ToString(),
                CouncilorImpression = intake.CouncilorImpression,
                ProblemPresentation = intake.ProblemPresentation,
                OtherFamilyDetails = intake.OtherFamilyDetails,


                // Add this: Load existing family members
                FamilyMembers = intake.FamilyMembers?.Select(fm => new FamilyMemberVm
                {
                    Name = fm.Name,
                    Age = fm.Age,
                    Relationship = fm.Relationship,
                    Comments = fm.Comments,

                }).ToList() ?? new List<FamilyMemberVm>(),
            };

            return View(vm);
        }
        // POST: Save all family data using IFormCollection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFamilyData([FromForm] IntakeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return to EditIntakeForm with the current model
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            var intakeForm = await _context.IntakeForms
                .Include(i => i.FamilyMembers)
                .FirstOrDefaultAsync(i => i.IntakeFormsId == model.IntakeId);

            if (intakeForm == null)
            {
                return NotFound();
            }

            try
            {
                // Remove existing family members
                _context.FamilyMembers.RemoveRange(intakeForm.FamilyMembers);

                // Add family members from the model
                if (model.FamilyMembers != null)
                {
                    foreach (var familyMember in model.FamilyMembers.Where(fm => !string.IsNullOrWhiteSpace(fm.Name)))
                    {
                        intakeForm.FamilyMembers.Add(new FamilyMember
                        {
                            Name = familyMember.Name,
                            Age = familyMember.Age,
                            Relationship = familyMember.Relationship,
                            Comments = familyMember.Comments,
                            IntakeFormId = intakeForm.IntakeFormsId
                        });
                    }
                }

                // Update other family details
                intakeForm.IntakeStatus = IntakeStatus.InProgress.ToString();
                intakeForm.OtherFamilyDetails = model.OtherFamilyDetails;
                // Update status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Family information saved successfully. Added {intakeForm.FamilyMembers.Count} family members.";
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "An error occurred while saving family information.";
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            return RedirectToAction("EditIntakeForm", new { id = intakeForm.IntakeFormsId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProblems(IntakeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            var intakeForm = await _context.IntakeForms
                .FirstOrDefaultAsync(i => i.IntakeFormsId == model.IntakeId);

            if (intakeForm == null)
            {
                return NotFound();
            }

            try
            {
                // Update presenting problems
                intakeForm.ProblemPresentation = model.ProblemPresentation;
                intakeForm.IntakeStatus = IntakeStatus.InProgress.ToString();
                _context.IntakeForms.Update(intakeForm);

                // Update status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Presenting problems saved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving presenting problems.";
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            return RedirectToAction("EditIntakeForm", new { id = intakeForm.IntakeFormsId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImpressions(IntakeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            var intakeForm = await _context.IntakeForms
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(i => i.IntakeFormsId == model.IntakeId);

            //TODO RETURN TEMDATA
            if (intakeForm == null)
            {
                return NotFound();
            }

            try
            {
                // Update counselor impressions
                intakeForm.CouncilorImpression = model.CouncilorImpression;
                intakeForm.IntakeStatus = IntakeStatus.InProgress.ToString();
                _context.IntakeForms.Update(intakeForm);

                // Update status
                await UpdatePatientStatus(intakeForm.PatientId);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Counselor impressions saved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving counselor impressions.";
                return RedirectToAction("EditIntakeForm", new { id = model.IntakeId });
            }

            return RedirectToAction("EditIntakeForm", new { id = intakeForm.IntakeFormsId });
        }

        //Helper method to update patient status
        private async Task UpdatePatientStatus(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(i => i.PatientId == id);

            if (patient == null)
                return;

            // All sections completed
            if (patient.PatientStatus == PatientStatusEnum.NewReferral.ToString())
            {
                patient.PatientStatus = PatientStatusEnum.Pending.ToString();
            }
            if (patient.PatientStatus == PatientStatusEnum.Pending.ToString())
            {
                patient.PatientStatus = PatientStatusEnum.Waitlisted.ToString();
            } 
         
            await _context.SaveChangesAsync();

        }

        //Action to Submit Intake form for assessment
        [HttpPost]
        public async Task<IActionResult> SubmitIntakeForm(int IntakeId)
        {
            // Logic to submit the intake form for assessment
            var intakeForm = await _context.IntakeForms.FindAsync(IntakeId);
            if (intakeForm == null)
            {
                return NotFound();
            }

            // Update the status to submitted
            intakeForm.IntakeStatus = IntakeStatus.Completed.ToString();
            _context.IntakeForms.Update(intakeForm);

            //update patient status
            await UpdatePatientStatus(intakeForm.PatientId);
            await _context.SaveChangesAsync();

            try
            {
                //pre populate scheduling tables
                var scheduling = new NewAppointment
                {
                    PatientId = intakeForm.PatientId,
                    Type = "Initial Assessment",
                    Status = SchedulingStatus.Pending.ToString()
                };
                await _context.NewAppointments
                .AddAsync(scheduling);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Intake form submitted for assessment successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting the intake form.";
                return RedirectToAction("EditIntakeForm", new { id = IntakeId });
            }
            return RedirectToAction("Index");
        }
    }
}