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
    public class ApprovalController : Controller
    {
        //Inject services here as needed
        private readonly SafehavenPMSContext _context;

        public ApprovalController(SafehavenPMSContext context)
        {
            _context = context;
        }

        // Action to list admissions or initial assessments with search, sort, paging
        public async Task<IActionResult> Index(string searchQuery, string status, string sortOrder, int page = 1, int pageSize = 10)
        {
            try
            {
                Console.WriteLine($"Index called: searchQuery='{searchQuery}', status='{status}', sortOrder='{sortOrder}', page={page}, pageSize={pageSize}");

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

                // Show patients with PendingApproval OR Admitted status
                var pendingApprovalStatus = PatientStatusEnum.PendingApproval.ToString();
                var admittedStatus = PatientStatusEnum.Admitted.ToString();
                var transferStatus = PatientStatusEnum.Discharged.ToString();
                Console.WriteLine("Filtering for PatientStatus = " + pendingApprovalStatus + " OR " + admittedStatus);

                // Filter by patient status (PendingApproval OR Admitted)
                query = query.Where(iaf => iaf.Patient != null &&
                    (iaf.Patient.PatientStatus == pendingApprovalStatus || iaf.Patient.PatientStatus == admittedStatus
                     || iaf.Patient.PatientStatus == transferStatus));

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

                // Apply sorting
                if (sortOrder == "ascending")
                {
                    query = query.OrderBy(iaf => iaf.CompletedAt ?? iaf.CreatedAt);
                    Console.WriteLine("Sorting ascending by completed/created");
                }
                else
                {
                    query = query.OrderByDescending(iaf => iaf.CompletedAt ?? iaf.CreatedAt);
                    Console.WriteLine("Sorting descending by completed/created");
                }

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

                    return new ApprovalViewModel
                    {
                        PatientId = patient?.PatientId ?? 0,
                        PatientName = patient != null ? $"{patient.Firstname} {patient.Lastname}" : "-",
                        CompletedAt = iaf.CompletedAt ?? iaf.UpdatedAt ?? iaf.CreatedAt,
                        Status = patient?.PatientStatus ?? "-",
                        Diagnosis = diagnosisText,
                        Recommendation = recommendationText
                    };
                }).ToList();

                Console.WriteLine($"Model items: {model.Count}");
                ViewBag.TotalPatientCount = model.Count;
                ViewBag.TotalPendingApprovalCount = statusCounts.FirstOrDefault(sc => sc.Status == pendingApprovalStatus)?.Count ?? 0;
                ViewBag.TotalAdmittedCount = statusCounts.FirstOrDefault(sc => sc.Status == admittedStatus)?.Count ?? 0;

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AdmissionController.Index ERROR: " + ex);
                Console.WriteLine($"  searchQuery='{searchQuery}', status='{status}', sortOrder='{sortOrder}', page={page}, pageSize={pageSize}");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        // helper to produce next CaseId in the format CASE-000001
        private async Task<string> GenerateNextCaseIdAsync()
        {
            // find the maximum numeric suffix used so far
            var lastCase = await _context.Admissions
                .Where(a => !string.IsNullOrEmpty(a.CaseId) && a.CaseId.StartsWith("CASE-"))
                .Select(a => a.CaseId)
                .OrderByDescending(c => c)
                .FirstOrDefaultAsync();

            int next = 1;
            if (!string.IsNullOrWhiteSpace(lastCase) && lastCase.Length >= 6)
            {
                var suffix = lastCase.Substring(5);
                if (int.TryParse(suffix, out var parsed))
                {
                    next = parsed + 1;
                }
            }
            return $"CASE-{next:000000}";
        }

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
                CaseId = await GenerateNextCaseIdAsync(),
                ProgramType = programType,
                AdmissionDate = DateTime.Now,
                CreatedAt = DateTime.Now,
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
    }
}