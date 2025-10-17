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
using SafehavenPMS.ViewModel.Approval;

namespace SafehavenPMS.Controllers
{
[Authorize]
    public class ApprovalController : Controller
    {
        //Inject services here as needed
        private readonly SafehavenPMSContext _context;

        public ApprovalController(SafehavenPMSContext context)
        {
            _context = context;
        }

        // Action to list admissions or initial assessments with search, sort, paging
        public async Task<IActionResult> Index(string searchQuery, string status, string sortOrder, string sortBy, int page = 1, int pageSize = 10)
         {
             try
             {
                 Console.WriteLine($"Index called: searchQuery='{searchQuery}', status='{status}', sortOrder='{sortOrder}', sortBy='{sortBy}', page={page}, pageSize={pageSize}");
 
                 // Log patient status counts
                 var statusCounts = await _context.Patients
                     .GroupBy(p => p.PatientStatus)
                     .Select(g => new { Status = g.Key, Count = g.Count() })
                     .ToListAsync();
                 Console.WriteLine("PatientStatusCounts:");
                 foreach (var sc in statusCounts) Console.WriteLine($"  Status='{sc.Status}' Count={sc.Count}");
 
                 // Base query with related data loaded
                 var query = _context.InitialAssessmentForms
                     .Include(iaf => iaf.Patient)
                     .Include(iaf => iaf.Diagnosis).ThenInclude(d => d.SubstanceUseEntries)
                     .Include(iaf => iaf.Recommendation)
                     .AsQueryable();
 
                // Prepare status values
                var pendingApprovalStatus = PatientStatusEnum.PendingApproval.ToString();
                var pendingAdmissionStatus = PatientStatusEnum.PendingAdmission.ToString();
                var dischargedStatus = PatientStatusEnum.Discharged.ToString();

                // Normalize incoming status filter and apply
                var normalizedStatus = status?.Trim();
                if (string.IsNullOrEmpty(normalizedStatus) || normalizedStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    // Default behaviour: show items that need approval (PendingApproval) and those already approved for admission (PendingAdmission)
                    Console.WriteLine("No explicit status requested - defaulting to PendingApproval OR PendingAdmission");
                    query = query.Where(iaf => iaf.Patient != null &&
                        (iaf.Patient.PatientStatus == pendingApprovalStatus || iaf.Patient.PatientStatus == pendingAdmissionStatus));
                }
                else
                {
                    Console.WriteLine($"Applying status filter: '{normalizedStatus}'");
                    var nsLower = normalizedStatus.ToLower();
                    if (nsLower == "completed")
                    {
                        // Completed = any status except NewIntake, InProgress, Discharged
                        var newIntake = PatientStatusEnum.NewIntake.ToString().ToLower();
                        var inProgress = PatientStatusEnum.InProgress.ToString().ToLower();
                        var discharged = PatientStatusEnum.Discharged.ToString().ToLower();

                        query = query.Where(iaf => iaf.Patient != null &&
                            iaf.Patient.PatientStatus != null &&
                            iaf.Patient.PatientStatus.ToLower() != newIntake &&
                            iaf.Patient.PatientStatus.ToLower() != inProgress &&
                            iaf.Patient.PatientStatus.ToLower() != discharged);
                    }
                    else
                    {
                        // Direct match (case-insensitive) to the PatientStatus string
                        query = query.Where(iaf => iaf.Patient != null &&
                            iaf.Patient.PatientStatus != null &&
                            iaf.Patient.PatientStatus.ToLower() == nsLower);
                    }
                }
 
                 // Apply search
                 if (!string.IsNullOrWhiteSpace(searchQuery))
                 {
                     var q = searchQuery.Trim();
                     Console.WriteLine($"Applying search filter: '{q}'");
                     query = query.Where(iaf =>
                     iaf.Patient.Firstname.Contains(q) ||
                     iaf.Patient.Lastname.Contains(q) ||
                     iaf.Patient.PatientId.ToString().Contains(q));
                 }
 
                // Apply sorting - support sortBy (Name | DateAdded) and sortOrder (ascending | descending)
                var so = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder.ToLower();
                var sb = string.IsNullOrEmpty(sortBy) ? "DateAdded" : sortBy;
                Console.WriteLine($"Sorting by '{sb}' order '{so}'");

                if (string.Equals(sb, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    query = so == "ascending"
                        ? query.OrderBy(iaf => iaf.Patient.Firstname).ThenBy(iaf => iaf.Patient.Lastname)
                        : query.OrderByDescending(iaf => iaf.Patient.Firstname).ThenByDescending(iaf => iaf.Patient.Lastname);
                }
                else
                {
                    // DateAdded or default -> use CompletedAt fallback to CreatedAt
                    query = so == "ascending"
                        ? query.OrderBy(iaf => iaf.CompletedAt ?? iaf.CreatedAt)
                        : query.OrderByDescending(iaf => iaf.CompletedAt ?? iaf.CreatedAt);
                }

                // Persist current filter/sort state for the view
                ViewBag.SearchQuery = searchQuery;
                ViewBag.Status = string.IsNullOrEmpty(status) ? "All" : status;
                ViewBag.SortOrder = so == "descending" ? "descending" : "ascending";
                ViewBag.SortBy = sb;
 
                 // Paging
                 int totalCount = await query.CountAsync();
                 Console.WriteLine($"Query COUNT result: {totalCount}");
                 int totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
                 var currentPage = Math.Max(1, Math.Min(page, totalPages));
 
                 var iafList = await query
                     .Skip((currentPage - 1) * pageSize)
                     .Take(pageSize > 0 ? pageSize : totalCount)
                     .ToListAsync();
 
                 Console.WriteLine($"Fetched iafList.Count = {iafList.Count}");
                 var iafIds = iafList.Select(i => i.InitialAssessmentFormId).ToList();
                 Console.WriteLine($"IAF ids: {string.Join(", ", iafIds)}");
 
                 // Map to view model using the already-included navigation properties
                 var model = iafList.Select(iaf =>
                 {
                     var patient = iaf.Patient;
 
                     var diag = iaf.Diagnosis;
                     var substances = diag?.SubstanceUseEntries?
                     .Where(s => !string.IsNullOrWhiteSpace(s.SubstanceName))
                     .Select(s => s.SubstanceName.Trim())
                     .Distinct()
                     .ToList();
 
                     var diagnosisText = (substances != null && substances.Any())
                     ? string.Join(", ", substances)
                     : "-";
 
                     var rec = iaf.Recommendation;
                     string recommendationText = "-";
                     if (rec != null)
                     {
                         var pt = rec.ProgramType?.Trim();
                         if (!string.IsNullOrWhiteSpace(pt))
                             recommendationText = pt;
                     }
 
                     Console.WriteLine($"IAF {iaf.InitialAssessmentFormId} -> DiagnosisId={(diag != null ? diag.DiagnosisId.ToString() : "null")}, SubstanceCount={(substances?.Count ?? 0)}, RecommendationId={(rec != null ? rec.RecommendationId.ToString() : "null")}");
                     var expectedDurationText = rec?.ExpectedDuration?.Trim();
                     if (string.IsNullOrWhiteSpace(expectedDurationText))
                         expectedDurationText = "-";
 
                     // IsDrugDependent = true when Diagnosis is present (not "-")
                     var isDrugDependent = !string.IsNullOrWhiteSpace(diagnosisText) && diagnosisText != "-";
  
                     return new ApprovalViewModel
                     {
                         PatientId = patient?.PatientId ?? 0,
                         PatientName = patient != null ? $"{patient.Firstname} {patient.Lastname}" : "-",
                         CompletedAt = iaf.CompletedAt ?? iaf.UpdatedAt ?? iaf.CreatedAt,
                         Status = patient?.PatientStatus ?? "-",
                         Diagnosis = diagnosisText,
                         Recommendation = recommendationText,
                         ExpectedDuration = expectedDurationText,
                         IsDrugDependent = isDrugDependent
                     };
                 }).ToList();
 
                 Console.WriteLine($"Model items: {model.Count}");
                ViewBag.TotalPatientCount = totalCount;
                 ViewBag.TotalPendingApprovalCount = statusCounts.FirstOrDefault(sc => sc.Status == pendingApprovalStatus)?.Count ?? 0;
                 ViewBag.TotalAdmittedCount = statusCounts.FirstOrDefault(sc => sc.Status == pendingAdmissionStatus)?.Count ?? 0;
 
                // pagination metadata for view
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = currentPage;
                ViewBag.PageSize = pageSize;
 
                 return View(model);
             }
             catch (Exception ex)
             {
                 Console.WriteLine("AdmissionController.Index ERROR: " + ex);
                 Console.WriteLine($"  searchQuery='{searchQuery}', status='{status}', sortOrder='{sortOrder}', page={page}, pageSize={pageSize}");
                 return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
             }
         }

         [HttpGet]
         public IActionResult SortBy(string sortBy, string sortOrder, string searchQuery, int page = 1, int pageSize = 10)
         {
             return RedirectToAction("Index", new { sortBy, sortOrder, searchQuery, page, pageSize });
         }

        //// helper to produce next CaseId in the format CASE-000001
        //private async Task<string> GenerateNextCaseIdAsync()
        //{
        //    // find the maximum numeric suffix used so far
        //    var lastCase = await _context.Admissions
        //        .Where(a => !string.IsNullOrEmpty(a.CaseId) && a.CaseId.StartsWith("CASE-"))
        //        .Select(a => a.CaseId)
        //        .OrderByDescending(c => c)
        //        .FirstOrDefaultAsync();

        //    int next = 1;
        //    if (!string.IsNullOrWhiteSpace(lastCase) && lastCase.Length >= 6)
        //    {
        //        var suffix = lastCase.Substring(5);
        //        if (int.TryParse(suffix, out var parsed))
        //        {
        //            next = parsed + 1;
        //        }
        //    }
        //    return $"CASE-{next:000000}";
        //}

        //Action for Residential Modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdmitPatient(int patientId, string programType)
        {
            if (patientId <= 0 || string.IsNullOrWhiteSpace(programType))
            {
                return BadRequest("Invalid patient ID or program type.");
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            // Update patient status to Admitted
            patient.PatientStatus = PatientStatusEnum.PendingAdmission.ToString();

            //Update admission Program type
            var newAdmission = new Admission
            {
                PatientId = patientId,
                // CaseId = await GenerateNextCaseIdAsync(),
                ProgramType = programType,
                ApprovalDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            //Save changes to database
            _context.Admissions.Add(newAdmission);
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            //Tempdata for Success message
            TempData["SuccessMessage"] = $"Patient {patientId} admitted to {programType} successfully.";

            // Log the admission action
            Console.WriteLine($"Patient {patientId} admitted to {programType} on {DateTime.Now}");

            // Redirect back to the Index view
            return RedirectToAction(nameof(Index));
        }

        //Action for Outpatient Modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargeToOutpatient(int patientId, string programType)
        {
            if (patientId <= 0 || string.IsNullOrWhiteSpace(programType))
            {
                return BadRequest("Invalid patient ID or program type.");
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            // Update patient status to Discharged
            patient.PatientStatus = PatientStatusEnum.Discharged.ToString();

            // Archive all patient forms
            await ArchivePatientFormsAsync(patientId);

            var dischargedPatient = new DischargedPatient
            {
                PatientId = patientId,
                ProgramType = programType,
                Reason = "Transferred",
                Status = PatientStatusEnum.Discharged.ToString(),
                CreatedBy = User.Identity?.Name ?? "System",
                DischargeDate = DateTime.Now
            };

            // Save changes to database
            _context.DischargedPatients.Add(dischargedPatient);
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            // Tempdata for Success message
            TempData["SuccessMessage"] = $"Patient {patientId} discharged to {programType} successfully.";

            // Log the discharge action
            Console.WriteLine($"Patient {patientId} discharged to {programType} on {DateTime.Now}");

            // Redirect back to the Index view
            return RedirectToAction(nameof(Index));
        }

        //Action for Community-based
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargeToCommunity(int patientId, string programType)
        {
            if (patientId <= 0 || string.IsNullOrWhiteSpace(programType))
            {
                return BadRequest("Invalid patient ID or program type.");
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            // Update patient status to Discharged
            patient.PatientStatus = PatientStatusEnum.Discharged.ToString();

            // Archive all patient forms
            await ArchivePatientFormsAsync(patientId);

            var dischargedPatient = new DischargedPatient
            {
                PatientId = patientId,
                ProgramType = programType,
                Reason = "Transferred",
                Status = PatientStatusEnum.Discharged.ToString(),
                CreatedBy = User.Identity?.Name ?? "System",
                DischargeDate = DateTime.Now
            };

            // Save changes to database
            _context.DischargedPatients.Add(dischargedPatient);
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            // Tempdata for Success message
            TempData["SuccessMessage"] = $"Patient {patientId} discharged to {programType} successfully.";

            // Log the discharge action
            Console.WriteLine($"Patient {patientId} discharged to {programType} on {DateTime.Now}");

            // Redirect back to the Index view
            return RedirectToAction(nameof(Index));
        }

        // Helper method to archive all patient forms when discharged
        private async Task ArchivePatientFormsAsync(int patientId)
        {
            try
            {
                // Intake Forms
                if (_context.IntakeForms != null)
                {
                    var intakeForms = await _context.IntakeForms
                        .Where(f => f.PatientId == patientId)
                        .ToListAsync();

                    foreach (var f in intakeForms)
                    {
                        f.Status = "Archived";
                        f.UpdatedAt = DateTime.UtcNow;
                        f.UpdatedBy = User.Identity?.Name;
                    }
                }

                // Initial Assessment Forms
                if (_context.InitialAssessmentForms != null)
                {
                    var initialAssessments = await _context.InitialAssessmentForms
                        .Where(f => f.PatientId == patientId)
                        .ToListAsync();

                    foreach (var f in initialAssessments)
                    {
                        f.UpdatedAt = DateTime.UtcNow;
                        f.UpdatedBy = User.Identity?.Name;
                    }
                }

                // Psychiatric Assessment Forms
                if (_context.PsychiatricAssessments != null)
                {
                    var psychAssessments = await _context.PsychiatricAssessments
                        .Where(f => f.PatientId == patientId)
                        .ToListAsync();

                    foreach (var f in psychAssessments)
                    {
                        f.Status = "Archived";
                        f.UpdatedAt = DateTime.UtcNow;
                        f.UpdatedBy = User.Identity?.Name;
                    }
                }

                // Interventions
                if (_context.Interventions != null)
                {
                    var interventions = await _context.Interventions
                        .Where(f => f.PatientId == patientId)
                        .ToListAsync();

                    foreach (var f in interventions)
                    {
                        f.Status = "Archived";
                    }
                }

                // Medication Orders
                if (_context.MedicationOrders != null)
                {
                    var medicationOrders = await _context.MedicationOrders
                        .Where(f => f.PatientId == patientId)
                        .ToListAsync();

                    foreach (var f in medicationOrders)
                    {
                        f.Status = "Archived";
                        f.UpdatedAt = DateTime.UtcNow;
                        f.UpdatedBy = User.Identity?.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't fail the discharge process
                Console.WriteLine($"Error archiving forms for patient {patientId}: {ex.Message}");
            }
        }
    }
}
