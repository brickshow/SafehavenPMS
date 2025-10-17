using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using SafehavenPMS.Services;
using SafehavenPMS.ViewModel;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SafehavenPMS.Controllers
{
    [Authorize]
    public class AdmissionController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly IEmailService _emailService;

        // ...existing code...
        public AdmissionController(SafehavenPMSContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                var query = _context.Admissions
                    .Include(iaf => iaf.Patient)
                    .AsQueryable();

                var admittedStatus = PatientStatusEnum.Admitted.ToString();
                var pendingStatus = PatientStatusEnum.PendingAdmission.ToString();

                // If a specific status filter was provided, apply it. Otherwise show PendingAdmission OR Admitted (legacy behavior).
                if (!string.IsNullOrWhiteSpace(status))
                {
                    Console.WriteLine($"Applying status filter: '{status}'");
                    query = query.Where(iaf => iaf.Patient != null && iaf.Patient.PatientStatus == status);
                }
                else
                {
                    Console.WriteLine($"No explicit status provided - defaulting to PendingAdmission OR Admitted");
                    query = query.Where(iaf => iaf.Patient != null &&
                        (iaf.Patient.PatientStatus == pendingStatus || iaf.Patient.PatientStatus == admittedStatus));
                }

                // Apply search (case-insensitive)
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var q = searchQuery.Trim().ToLower();
                    Console.WriteLine($"Applying search filter: '{q}'");
                    query = query.Where(iaf =>
                        ((iaf.Patient.Firstname ?? "").ToLower().Contains(q)) ||
                        ((iaf.Patient.Lastname ?? "").ToLower().Contains(q)) ||
                        iaf.Patient.PatientId.ToString().Contains(q));
                }

                // Determine sort field and order
                var so = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder.ToLower();
                var sb = string.IsNullOrEmpty(sortBy) ? "ApprovalDate" : sortBy;

                Console.WriteLine($"Sorting by '{sb}' order '{so}'");

                if (string.Equals(sb, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    query = so == "ascending"
                        ? query.OrderBy(iaf => iaf.Patient.Firstname).ThenBy(iaf => iaf.Patient.Lastname)
                        : query.OrderByDescending(iaf => iaf.Patient.Firstname).ThenByDescending(iaf => iaf.Patient.Lastname);
                }
                else if (string.Equals(sb, "AdmissionDate", StringComparison.OrdinalIgnoreCase) || string.Equals(sb, "CreatedAt", StringComparison.OrdinalIgnoreCase))
                {
                    query = so == "ascending"
                        ? query.OrderBy(iaf => iaf.CreatedAt)
                        : query.OrderByDescending(iaf => iaf.CreatedAt);
                }
                else // default/ApprovalDate
                {
                    query = so == "ascending"
                        ? query.OrderBy(iaf => iaf.ApprovalDate)
                        : query.OrderByDescending(iaf => iaf.ApprovalDate);
                }

                // Paging
                int totalCount = await query.CountAsync();
                Console.WriteLine($"Query COUNT result: {totalCount}");
                int totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
                var currentPage = Math.Max(1, Math.Min(page, totalPages));

                var admissionsList = await query
                    .Include(a => a.Patient)
                        .ThenInclude(pt => pt.ClinicalStaffPatients)
                            .ThenInclude(csp => csp.ClinicalStaff)
                    .Skip((currentPage - 1) * (pageSize > 0 ? pageSize : totalCount))
                    .Take(pageSize > 0 ? pageSize : totalCount)
                    .ToListAsync();

                // Map to view model using the already-included navigation properties
                var model = admissionsList.Select(p =>
                {
                    var name = p.Patient != null ? $"{p.Patient.Firstname} {p.Patient.Lastname}" : "-";

                    // build clinical team from patient's ClinicalStaffPatients
                    var clinicalTeam = (p.Patient?.ClinicalStaffPatients ?? Enumerable.Empty<ClinicalStaffPatient>())
                        .Where(c => c.ClinicalStaff != null)
                        .Select(c => new ClinicalTeamMember
                        {
                            Id = c.ClinicalStaff.ClinicalStaffID,
                            FullName = $"{c.ClinicalStaff.Firstname} {c.ClinicalStaff.Lastname}".Trim(),
                            Position = c.ClinicalStaff.Position ?? "",
                            ProfilePictureURL = c.ClinicalStaff.ProfilePictureURL ?? ""  // safe fallback
                        })
                        .ToList();

                    return new AdmitPatientViewModel
                    {
                        PatientId = p.PatientId,
                        FullName = name,
                        CreatedAt = p.CreatedAt,
                        ApprovalDate = p.ApprovalDate,
                        Status = p.Patient?.PatientStatus,
                        ClinicalTeam = clinicalTeam
                    };
                }).ToList();

                Console.WriteLine($"Model items: {model.Count}");

                // Provide view state so filter UI stays in sync
                ViewBag.TotalPatientCount = totalCount;
                ViewBag.TotalPendingApprovalCount = statusCounts.FirstOrDefault(sc => sc.Status == pendingStatus)?.Count ?? 0;
                ViewBag.TotalAdmittedCount = statusCounts.FirstOrDefault(sc => sc.Status == admittedStatus)?.Count ?? 0;

                ViewBag.ServiceTypes = new SelectList(await _context.ServiceTypes.ToListAsync(), "ServiceTypeId", "ServiceName");
                ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "ServiceId", "ServiceName");

                ViewBag.SearchQuery = searchQuery;
                ViewBag.Status = status;
                ViewBag.SortBy = sb;
                ViewBag.SortOrder = so;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = currentPage;
                ViewBag.TotalPages = totalPages;

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AdmissionController.Index ERROR: " + ex);
                Console.WriteLine($"  searchQuery='{searchQuery}', status='{status}', sortOrder='{sortOrder}', sortBy='{sortBy}', page={page}, pageSize={pageSize}");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public IActionResult SortBy(string sortBy, string sortOrder, string searchQuery, string status, int page = 1, int pageSize = 10)
        {
            return RedirectToAction("Index", new { sortBy, sortOrder, searchQuery, status, page, pageSize });
        }

        // GET: AdmitPatient page to show dropdown of patients with PendingReview
        [HttpGet]
        public async Task<IActionResult> AdmitPatient(int patientId)
        {
            // Populate clinical staff dropdowns used by the view
            await PopulateClinicalStaffDropdowns();

            // Get patients with PendingReview status for selection list
            var patients = await _context.Patients
                                .Include(a => a.Admissions)
                                .FirstOrDefaultAsync(i => i.PatientId == patientId);

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
                    vm.AdmissionId = patient.Admissions?.FirstOrDefault(a => a.Status == "Active")?.AdmissionId ?? 0;
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

                    // // NEW: populate clinical team members for display
                    // vm.ClinicalTeam = patient.ClinicalStaffPatients?
                    //     .Where(c => c.ClinicalStaff != null)
                    //     .Select(c => new AdmitPatientViewModel.ClinicalTeamMember
                    //     {
                    //         Id = c.ClinicalStaff.ClinicalStaffID,
                    //         FullName = $"{c.ClinicalStaff.Firstname} {c.ClinicalStaff.Lastname}".Trim(),
                    //         Position = c.ClinicalStaff.Position ?? ""
                    //     }).ToList() ?? new List<AdmitPatientViewModel.ClinicalTeamMember>();
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

          [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdmitPatient(AdmitPatientViewModel model)
        {
            // Require family/responsible person on admission
            if (string.IsNullOrWhiteSpace(model.FamilyName))
            {
                ModelState.AddModelError("FamilyName", "Family / Responsible person is required.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateClinicalStaffDropdowns();
                // flag for the view to show modal (view also checks ModelState)
                ViewBag.ShowFamilyModal = true;
                return View(model);
            }

            var patient = await _context.Patients.FindAsync(model.PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Try to reuse an existing active admission for this patient to avoid duplicates
                var admission = await _context.Admissions
                    .FirstOrDefaultAsync(a => a.PatientId == model.PatientId);

                if (admission == null)
                {
                    admission = new Admission
                    {
                        PatientId = model.PatientId,
                        PhysicianId = model.PhysicianId,
                        PsychologistId = model.PsychologistId,  
                        PsychometricianId = model.PsychometricianId,
                        SocialWorkerId = model.SocialWorkerId,
                        RecoveryCoachId = model.RecoveryCoachId,
                        CreatedAt = DateTime.Now,
                        Status = "Active" // adjust as appropriate
                    };

                    _context.Admissions.Add(admission);
                }
                else
                {
                    // update fields on existing admission (no new row will be created)
                    admission.PhysicianId = model.PhysicianId;
                    admission.PsychologistId = model.PsychologistId;
                    admission.PsychometricianId = model.PsychometricianId;
                    admission.SocialWorkerId = model.SocialWorkerId;
                    admission.RecoveryCoachId = model.RecoveryCoachId;
                    admission.CreatedAt = DateTime.Now;
                    _context.Admissions.Update(admission);
                }

                // Prepare selected staff ids (unique)
                var selectedStaffIds = new int?[]
                {
                    model.PhysicianId,
                    model.PsychologistId,
                    model.PsychometricianId,
                    model.SocialWorkerId,
                    model.RecoveryCoachId
                }
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

                // Query DB for existing ClinicalStaffPatient entries for this patient to avoid duplicates
                var persistedStaffIds = await _context.ClinicalStaffPatients
                    .Where(c => c.PatientId == model.PatientId)
                    .Select(c => c.ClinicalStaffId)
                    .ToListAsync();

                var toAddIds = selectedStaffIds.Except(persistedStaffIds).ToList();

                if (toAddIds.Any())
                {
                    var joins = toAddIds
                        .Select(id => new ClinicalStaffPatient
                        {
                            PatientId = model.PatientId,
                            ClinicalStaffId = id
                        })
                        .ToList();

                    await _context.ClinicalStaffPatients.AddRangeAsync(joins);
                }

                //Select Patieny RefId
                

                // Create family portal user only if one doesn't already exist for this patient or email
                if (!string.IsNullOrWhiteSpace(model.FamilyEmail))
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.PatientId == model.PatientId);

                    if (existingUser == null)
                    {
                        var username = patient.PatientRefId.ToString();
                        var password = GeneratePassword(10);

                        var user = new User
                        {
                            Username = username,
                            Email = model.FamilyEmail,
                            Fullname = model.FamilyName ?? string.Empty,
                            Role = "Family",
                            IsActive = true,
                            PatientId = model.PatientId,
                            CreatedAt = DateTime.UtcNow
                        };

                        var hasher = new PasswordHasher<User>();
                        user.PasswordHash = hasher.HashPassword(user, password);

                        _context.Users.Add(user);

                        // Send credentials (best-effort; do not fail whole transaction if email fails)
                        try
                        {
                            await _emailService.SendStaffCredentialsAsync(user.Email, user.Username, password, model.FamilyName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Email send failed: " + ex);
                        }
                    }
                    else
                    {
                        // Optionally update email if different and existing user is tied to same patient
                        if (existingUser.PatientId == model.PatientId && existingUser.Email != model.FamilyEmail)
                        {
                            existingUser.Email = model.FamilyEmail;
                            _context.Users.Update(existingUser);
                        }
                        // Ensure Fullname is stored/updated
                        if (existingUser.PatientId == model.PatientId && !string.IsNullOrWhiteSpace(model.FamilyName) && existingUser.Fullname != model.FamilyName)
                        {
                            existingUser.Fullname = model.FamilyName;
                            _context.Users.Update(existingUser);
                        }
                    }
                }

                // update patient status to Admitted
                patient.PatientStatus = PatientStatusEnum.Admitted.ToString();
                _context.Patients.Update(patient);

                // Pre-populate NewAppointment with PsychiatricAssesment if not already present for this admission
                var hasPsychAssessment = await _context.NewAppointments
                    .AnyAsync(a => a.PatientId == patient.PatientId && a.Type == "Psychiatric Assesment");
    
                if (!hasPsychAssessment)
                {
                    var psychAssessment = new NewAppointment
                    {
                        PatientId = patient.PatientId,
                        Type = "Psychiatric Assessment",
                        Status = SchedulingStatus.Pending.ToString(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = User?.Identity?.Name ?? "System"
                    };
                    await _context.NewAppointments.AddAsync(psychAssessment);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["SuccessMessage"] = "Patient admitted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine("AdmitPatient failed: " + ex);
                ModelState.AddModelError("", "Unable to admit patient. See logs.");
                await PopulateClinicalStaffDropdowns();
                return View(model);
            }
        }

        // helper: generate a reasonably strong random password
        private static string GeneratePassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*";
            var data = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[data[i] % chars.Length];
            }
            return new string(result);
        }

        // helper: PBKDF2 hash using provided Base64 salt
        private static string HashPassword(string password, string base64Salt)
        {
            var salt = Convert.FromBase64String(base64Salt);
            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hashed = derive.GetBytes(32);
            return Convert.ToBase64String(hashed);
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

        // ...existing code...
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdmitPatientViewModel model)
        {
            Console.WriteLine($"Edit POST start: id={id} model.PatientId={model?.PatientId} " +
                            $"PhysicianId={model?.PhysicianId} PsychologistId={model?.PsychologistId} " +
                            $"PsychometricianId={model?.PsychometricianId} SocialWorkerId={model?.SocialWorkerId} " +
                            $"RecoveryCoachId={model?.RecoveryCoachId}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState invalid");
                await PopulateClinicalStaffDropdowns();
                return View(model);
            }

            // Load admission including patient and existing clinical staff joins
            var admission = await _context.Admissions
                .Include(a => a.Patient)
                .Include(a => a.ClinicalStaffPatients)
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null)
            {
                Console.WriteLine($"Admission not found for id={id}");
                return NotFound();
            }

            Console.WriteLine($"Loaded admission: AdmissionId={admission.AdmissionId}, PatientId={admission.PatientId}, ExistingJoins={admission.ClinicalStaffPatients?.Count ?? 0}");

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update admission staff selections
                admission.PhysicianId = model.PhysicianId;
                admission.PsychologistId = model.PsychologistId;
                admission.PsychometricianId = model.PsychometricianId;
                admission.SocialWorkerId = model.SocialWorkerId;
                admission.RecoveryCoachId = model.RecoveryCoachId;

                // Optional: update patient fields if present on model
                if (admission.Patient != null)
                {
                    _context.Patients.Update(admission.Patient);
                    Console.WriteLine($"Patient updated in context: PatientId={admission.Patient.PatientId}");
                }

                // Sync ClinicalStaffPatients join table using PatientId
                var selectedStaffIds = new int?[]
                {
                    model.PhysicianId,
                    model.PsychologistId,
                    model.PsychometricianId,
                    model.SocialWorkerId,
                    model.RecoveryCoachId
                }
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

                Console.WriteLine("SelectedStaffIds: " + (selectedStaffIds.Any() ? string.Join(",", selectedStaffIds) : "(none)"));

                var existingJoins = admission.ClinicalStaffPatients?.ToList() ?? new List<ClinicalStaffPatient>();
                Console.WriteLine("ExistingJoins:");
                foreach (var j in existingJoins)
                {
                    Console.WriteLine($"  Join PatientId={j.PatientId} ClinicalStaffId={j.ClinicalStaffId}");
                }

                // remove joins that are no longer selected
                var toRemove = existingJoins.Where(j => !selectedStaffIds.Contains(j.ClinicalStaffId)).ToList();
                Console.WriteLine($"ToRemove count: {toRemove.Count}");
                foreach (var r in toRemove) Console.WriteLine($"  Remove ClinicalStaffId={r.ClinicalStaffId}");

                if (toRemove.Any())
                {
                    _context.ClinicalStaffPatients.RemoveRange(toRemove);
                }

                // --- REPLACE: robust add logic that checks DB for persisted rows ---
                // Query DB for currently persisted ClinicalStaffIds for this patient (avoid race/track issues)
                var persistedStaffIds = await _context.ClinicalStaffPatients
                    .Where(c => c.PatientId == admission.PatientId)
                    .Select(c => c.ClinicalStaffId)
                    .ToListAsync();

                Console.WriteLine("PersistedStaffIds: " + (persistedStaffIds.Any() ? string.Join(",", persistedStaffIds) : "(none)"));

                // Only add IDs that are selected but not already persisted
                var toAddIds = selectedStaffIds.Where(id => !persistedStaffIds.Contains(id)).ToList();

                var toAdd = toAddIds
                    .Select(id => new ClinicalStaffPatient
                    {
                        PatientId = admission.PatientId,
                        ClinicalStaffId = id,
                    })
                    .ToList();

                Console.WriteLine($"ToAdd count: {toAdd.Count}");
                foreach (var a in toAdd) Console.WriteLine($"  Add ClinicalStaffId={a.ClinicalStaffId}");

                if (toAdd.Any())
                {
                    await _context.ClinicalStaffPatients.AddRangeAsync(toAdd);
                }
                // --- END REPLACE ---
                
                _context.Admissions.Update(admission);
                Console.WriteLine("Saving changes...");
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                Console.WriteLine("Save/Commit succeeded for Edit.");
                TempData["SuccessMessage"] = "Admission updated successfully!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine("DbUpdateException in Edit: " + ex);
                ModelState.AddModelError("", "Unable to save changes. Please try again.");
                await PopulateClinicalStaffDropdowns();
                return View(model);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine("Exception in Edit: " + ex);
                ModelState.AddModelError("", "Unable to save changes. Please try again.");
                await PopulateClinicalStaffDropdowns();
                return View(model);
            }
        }
    }
}

