using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System;
using System.Linq;
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
            var transferStatus = PatientStatusEnum.Closed.ToString();
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

                return new AdmitPatientViewModel
                {
                PatientId = patient?.PatientId ?? 0,
                FullName = patient != null ? $"{patient.Firstname} {patient.Lastname}" : "-",
                CompletedDate = iaf.CompletedAt ?? iaf.UpdatedAt ?? iaf.CreatedAt,
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

        // // SearchPatient: POST action to find a patient by id with PendingReview status
        // [HttpPost]
        // public async Task<IActionResult> SearchPatient(int searchQuery)
        // {
        //     // Find patient including their clinical staff assignments
        //     var patient = await _context.Patients
        //         .Include(p => p.ClinicalStaffPatients)
        //             .ThenInclude(csp => csp.ClinicalStaff)
        //         .FirstOrDefaultAsync(p => p.PatientId == searchQuery &&
        //                                   p.PatientStatus == Enum.PatientStatusEnum.Ad.ToString()); // ensure status is PendingReview

        //     if (patient == null)
        //         return View("AdmitPatient", new AdmitPatientViewModel()); // return empty VM if not found

        //     // Choose first associated physician if any
        //     var physician = patient.ClinicalStaffPatients
        //         .Select(csp => csp.ClinicalStaff)
        //         .FirstOrDefault();

        //     // Map patient data to AdmitPatientViewModel for the AdmitPatient view
        //     var vm = new AdmitPatientViewModel
        //     {
        //         PatientId = patient.PatientId,
        //         FullName = $"{patient.Firstname} {patient.MiddleName} {patient.Lastname}".Trim(),
        //         Sex = patient.Sex,
        //         DOB = patient.DateOfBirth,
        //         EducationalAttainment = patient.Education,
        //         Occupation = patient.Occupation,
        //         Religion = patient.Religion,
        //         PhoneNumber = patient.PhoneNumber,
        //         PhysicianId = physician?.ClinicalStaffID,
        //         PhysicianName = physician != null ? $"{physician.Firstname} {physician.Lastname}" : ""
        //     };

        //     await PopulateClinicalStaffDropdowns(); // fill dropdown lists for view
        //     return View("AdmitPatient", vm); // return AdmitPatient view with VM
        // }

        // GET: AdmitPatient page to show dropdown of patients with PendingReview
        [HttpGet]
        public async Task<IActionResult> AdmitPatient(int patientId)
        {
            // Populate clinical staff dropdowns used by the view
            await PopulateClinicalStaffDropdowns();

            // Get patients with PendingReview status for selection list
            var patients = await _context.Patients.FirstOrDefaultAsync(i => i.PatientId == patientId);

            var vm = new AdmitPatientViewModel();

            if (patientId > 0)
            {
                // Load patient and their clinical-staff associations
                var patient = await _context.Patients
                    .Include(p => p.ClinicalStaffPatients)
                        .ThenInclude(csp => csp.ClinicalStaff)
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                // Calculate age from DoB (use helper)
                var ageText = CalculateAge(patient?.DateOfBirth);

                if (patient != null)
                {
                    vm.PatientId = patient.PatientId;
                    vm.FullName = $"{patient.Firstname} {patient.MiddleName} {patient.Lastname}".Trim();
                    vm.Sex = patient.Sex;
                    vm.Age = ageText;
                    vm.EducationalAttainment = patient.Education;
                    vm.Occupation = patient.Occupation;
                    vm.Religion = patient.Religion;
                    vm.PhoneNumber = patient.PhoneNumber;
                    vm.Address = string.IsNullOrWhiteSpace(patient.Address) ? "-" : patient.Address;

                    // Prefill per-role selects from ClinicalStaffPatients join entries (if present)
                    vm.PhysicianId = patient.ClinicalStaffPatients?
                        .Where(c => c.ClinicalStaff != null && c.ClinicalStaff.Position == "Physician")
                        .Select(c => c.ClinicalStaff.ClinicalStaffID)
                        .FirstOrDefault();



                    // Provide a readable physician name for the read-only field in the view
                    var physician = patient.ClinicalStaffPatients?
                        .Select(c => c.ClinicalStaff)
                        .FirstOrDefault(s => s != null && s.Position == "Physician")
                        ?? patient.ClinicalStaffPatients?.Select(c => c.ClinicalStaff).FirstOrDefault();

                    if (physician != null)
                        vm.PhysicianName = $"{physician.Firstname} {physician.Lastname}";
                }
            }

            return View(vm);
        }

         private string CalculateAge(DateTime? dob)
        {
            if (!dob.HasValue) return "-";
            var today = DateTime.Today;
            int years = today.Year - dob.Value.Year;
            // if birthday hasn't occurred yet this year, subtract one
            if (dob.Value.Date > today.AddYears(-years)) years--;
            if (years < 0) years = 0;
            return years.ToString();
        }

        // POST: AdmitPatient - create an admission for a patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdmitPatient(AdmitPatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState) // log each validation error
                {
                    var key = state.Key;
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }
                await PopulateClinicalStaffDropdowns(); // repopulate dropdowns for redisplay
                return View(model); // return view with validation messages
            }

            var patient = await _context.Patients
                .Include(p => p.ClinicalStaffPatients)
                .Include(i => i.InitialAssessmentForms)
                    .ThenInclude(r => r.Recommendation)
                .FirstOrDefaultAsync(p => p.PatientId == model.PatientId);

            if (patient == null)
            {
                ModelState.AddModelError("", "Patient not found.");
                await PopulateClinicalStaffDropdowns();
                return View(model);
            }

            // Generate next CaseId "CASE-000001"
            var newCaseId = await GenerateNextCaseIdAsync();

            //Get the program type
            var ProgramType = patient.InitialAssessmentForms?
                .OrderByDescending(iaf => iaf.CreatedAt)
                .FirstOrDefault()?
                .Recommendation?.ProgramType ?? "-";

            // Build admission entity
            var admission = new Admission
            {
                CaseId = newCaseId,
                PatientId = model.PatientId,
                PhysicianId = model.PhysicianId,
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
                CreatedBy = User?.Identity?.Name ?? "System",
                Status = AdmissionStatus.Active.ToString(),
                ProgramType = ProgramType,
                CurrentFacility = "Safehaven Rehabilitation Center" // default facility on admission
            };

            // Use transaction to ensure consistency when creating admission + clinical staff link + updating patient
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Add ClinicalStaffPatient entries for selected roles (avoid duplicates)
                    var staffIds = new int?[]
                    {
                        model.PhysicianId,
                        model.PsychologistId,
                        model.PsychometricianId,
                        model.SocialWorkerId,
                        model.RecoveryCoachId
                    }.Where(id => id.HasValue).Select(id => id!.Value).Distinct();

                    foreach (var staffId in staffIds)
                    {
                        var exists = await _context.ClinicalStaffPatients
                            .AnyAsync(csp => csp.PatientId == patient.PatientId && csp.ClinicalStaffId == staffId);

                        if (!exists)
                        {
                            _context.ClinicalStaffPatients.Add(new ClinicalStaffPatient
                            {
                                PatientId = patient.PatientId,
                                ClinicalStaffId = staffId
                            });
                        }
                    }

                    // Update patient status
                    patient.PatientStatus = PatientStatusEnum.Admitted.ToString();
                    _context.Patients.Update(patient);

                    // Add admission record
                    _context.Admissions.Add(admission);

                    // Prepopulate Psychiatric fields
                    var psychiatricAssessment = new PsychiatricAssessment
                    {
                        PatientId = patient.PatientId,
                        Type = "Psychiatric Assessment",
                        Date = DateTime.Now,
                        Time = DateTime.Now.ToString("hh:mm tt"),
                        Status = "Pending",
                        CreatedAt = DateTime.Now,
                        CreatedBy = User?.Identity?.Name ?? "System"
                    };
                    _context.PsychiatricAssessments.Add(psychiatricAssessment);

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error admitting patient: " + ex);
                    await tx.RollbackAsync();
                    ModelState.AddModelError("", "Unable to admit patient. Please try again.");
                    await PopulateClinicalStaffDropdowns();
                    return View(model);
                }
            }

            TempData["SuccessMessage"] = $"Patient {model.FullName} admitted successfully!";
            return RedirectToAction("Index");
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

        // Helper to populate clinical staff dropdowns used across views
        private async Task PopulateClinicalStaffDropdowns()
        {
            var allStaff = await _context.ClinicalStaffs.ToListAsync(); // get all staff

            // Create ViewBag.Physicians list filtered by Position
            ViewBag.Physicians = allStaff
               .Where(s => s.Position == "Physician")
               .Select(s => new SelectListItem
               {
                   Value = s.ClinicalStaffID.ToString(),
                   Text = s.Firstname + " " + s.Lastname
               }).ToList();

            // Create ViewBag.Psychiatrists
            ViewBag.Psychiatrists = allStaff
                .Where(s => s.Position == "Psychiatrist")
                .Select(s => new SelectListItem
                {
                   Value = s.ClinicalStaffID.ToString(),
                   Text = s.Firstname + " " + s.Lastname
                }).ToList();

            // Create ViewBag.Psychologists
            ViewBag.Psychologists = allStaff
                .Where(s => s.Position == "Psychologist")
                .Select(s => new SelectListItem
                {
                   Value = s.ClinicalStaffID.ToString(),
                   Text = s.Firstname + " " + s.Lastname
                }).ToList();

            // Create ViewBag.Psychometricians
            ViewBag.Psychometricians = allStaff
                .Where(s => s.Position == "Psychometrician")
                .Select(s => new SelectListItem
                {
                   Value = s.ClinicalStaffID.ToString(),
                   Text = s.Firstname + " " + s.Lastname
                }).ToList();

            // Create ViewBag.SocialWorkers
            ViewBag.SocialWorkers = allStaff
                .Where(s => s.Position == "Social Worker")
                .Select(s => new SelectListItem
                {
                   Value = s.ClinicalStaffID.ToString(),
                   Text = s.Firstname + " " + s.Lastname
                }).ToList();

            // Create ViewBag.RecoveryCoaches
            ViewBag.RecoveryCoaches = allStaff
                .Where(s => s.Position == "Recovery Coach")
                .Select(s => new SelectListItem
                {
                   Value = s.ClinicalStaffID.ToString(),
                   Text = s.Firstname + " " + s.Lastname
                }).ToList();
        }

        // GET: Edit admission by id
        public async Task<IActionResult> Edit(int id)
        {
            var admission = await _context.Admissions
                .Include(a => a.Patient) // include patient to show related info
                .FirstOrDefaultAsync(a => a.AdmissionId == id); // find admission by id

            if (admission == null)
            {
                return NotFound(); // return 404 if not found
            }

            // Map admission and patient details to view model
            var vm = new AdmitPatientViewModel
            {
                AdmissionId = admission.AdmissionId,
                PatientId = admission.PatientId,
                // populate selected staff ids so selects will show current values
                PhysicianId = admission.PhysicianId,
                PsychologistId = admission.PsychologistId,
                PsychometricianId = admission.PsychometricianId,
                SocialWorkerId = admission.SocialWorkerId,
                RecoveryCoachId = admission.RecoveryCoachId,

                //Patient Information
                FullName = admission.Patient != null ? $"{admission.Patient.Firstname} {admission.Patient.Lastname}".Trim() : "-",
                Age = CalculateAge(admission?.Patient?.DateOfBirth),
                Sex = admission.Patient != null ? admission.Patient.Sex : "-",
                Occupation = admission.Patient != null ? admission.Patient.Occupation : "-",
                Address = admission.Patient != null ? admission.Patient.Address : "-"
            };

            // populate staff display names for the view (optional helper fields on the VM)
            if (vm.PhysicianId > 0)
            {
                var s = await _context.ClinicalStaffs.FirstOrDefaultAsync(x => x.ClinicalStaffID == vm.PhysicianId);
                vm.PhysicianName = s != null ? $"{s.Firstname} {s.Lastname}" : "";
            }
            if (vm.PsychiatristId > 0)
            {
                var s = await _context.ClinicalStaffs.FirstOrDefaultAsync(x => x.ClinicalStaffID == vm.PsychiatristId);
                vm.PsychiatristName = s != null ? $"{s.Firstname} {s.Lastname}" : "";
            }
            if (vm.PsychologistId > 0)
            {
                var s = await _context.ClinicalStaffs.FirstOrDefaultAsync(x => x.ClinicalStaffID == vm.PsychologistId);
                vm.PsychologistName = s != null ? $"{s.Firstname} {s.Lastname}" : "";
            }
            if (vm.PsychometricianId > 0)
            {
                var s = await _context.ClinicalStaffs.FirstOrDefaultAsync(x => x.ClinicalStaffID == vm.PsychometricianId);
                vm.PsychometricianName = s != null ? $"{s.Firstname} {s.Lastname}" : "";
            }
            if (vm.SocialWorkerId > 0)
            {
                var s = await _context.ClinicalStaffs.FirstOrDefaultAsync(x => x.ClinicalStaffID == vm.SocialWorkerId);
                vm.SocialWorkerName = s != null ? $"{s.Firstname} {s.Lastname}" : "";
            }
            if (vm.RecoveryCoachId > 0)
            {
                var s = await _context.ClinicalStaffs.FirstOrDefaultAsync(x => x.ClinicalStaffID == vm.RecoveryCoachId);
                vm.RecoveryCoachName = s != null ? $"{s.Firstname} {s.Lastname}" : "";
            }

            await PopulateClinicalStaffDropdowns(); // populate dropdowns for view
            return View(vm); // return edit view with VM
        }

        // POST: Edit admission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdmitPatientViewModel model)
        {
            if (!ModelState.IsValid) // validate model
            {
                await PopulateClinicalStaffDropdowns(); // repopulate dropdowns for redisplay
                return View(model); // return view with validation errors
            }

            // Load admission including patient in case patient fields need to be updated
            var admission = await _context.Admissions
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null)
            {
                return NotFound(); // admission not found
            }

            // Update admission fields from posted model
            admission.FamilyName = model.FamilyName;
            admission.FamilyRelationship = model.FamilyRelationship;
            admission.FamilyPhone = model.FamilyPhone;
            admission.FamilyEmail = model.FamilyEmail;
            admission.ActivatePortal = model.ActivatePortal;

            // Optional: update patient fields if desired (commented out)
            // admission.Patient.Occupation = model.Occupation;
            // admission.Patient.Religion = model.Religion;
            // admission.Patient.PhoneNumber = model.PhoneNumber;

            try
            {
                _context.Update(admission); // mark admission modified
                await _context.SaveChangesAsync(); // save changes to DB

                TempData["SuccessMessage"] = "Admission updated successfully!"; // success message
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Unable to save changes. Please try again."); // show generic error
                Console.WriteLine(ex); // log exception
                await PopulateClinicalStaffDropdowns(); // repopulate dropdowns for view
                return View(model); // return view with error
            }

            return RedirectToAction("Index"); // redirect to list after success
        }

        // POST: Transfer patient to another facility
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(AdmitPatientViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // load current admission first so we can use its CurrentFacility as FromFacility
                var admission = await _context.Admissions.FirstOrDefaultAsync(a => a.PatientId == model.PatientId);

                var fromFacility = admission?.CurrentFacility ?? "Safehaven Rehabilitation Center";

                // 1) insert transfer audit
                var transfer = new PatientTransfer
                {
                    PatientId = model.PatientId,
                    FromFacility = fromFacility,
                    ToFacility = model.ReceivingFacility,
                    ProgramType = model.ProgramType,
                    Reason = model.Reason,
                    CreatedBy = User?.Identity?.Name ?? "System",
                    CreatedAt = DateTime.UtcNow
                };

                _context.PatientTransfers.Add(transfer);
                await _context.SaveChangesAsync();

                // 2) update admission current info (if admission exists)
                if (admission != null)
                {
                    admission.CurrentFacility = model.ReceivingFacility;
                    admission.ProgramType = model.ProgramType;
                    admission.Status = "Transferred";
                    _context.Admissions.Update(admission);
                    await _context.SaveChangesAsync();
                }

                // 3) update patient status to Closed after transfer
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == model.PatientId);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.Closed.ToString();
                    _context.Patients.Update(patient);
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                TempData["SuccessMessage"] = "Transfer saved.";
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error saving transfer: " + ex);
                await tx.RollbackAsync();
                TempData["Error"] = "Unable to save transfer.";
            }
            return RedirectToAction("Index");
        }
    }
}
