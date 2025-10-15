using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.ViewModel.PatientProfile;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using SafehavenPMS.Services;
using SafehavenPMS.Models;
using Microsoft.Data.SqlClient;
using SafehavenPMS.ViewModel.Assessment;
using SafehavenPMS.Enum;

namespace SafehavenPMS.Controllers
{
[Authorize]
    public class PatientProfileController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly CloudinaryServices _cloudSvc;
        private readonly ActivityLogService _activityService;

        // Add helper for full name
        private static string GetPatientFullName(Patient p) => $"{p.Firstname} {p.Lastname}";

        //Constructor
        public PatientProfileController(SafehavenPMSContext context, CloudinaryServices cloudSvc, ActivityLogService activityService)
        {
            _context = context;
            _cloudSvc = cloudSvc;
            _activityService = activityService;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Patient ID not found.";
                return View(new PatientProfilePageViewModel());
            }

            var patient = await _context.Patients
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(s => s.ClinicalStaff)
                .Include(c => c.IntakeForm)
                    .ThenInclude(i => i.FamilyMembers)
                .FirstOrDefaultAsync(i => i.PatientId == id);

            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return View(new PatientProfilePageViewModel());
            }

            var assessment = await _context.PsychiatricAssessments
                .Include(a => a.ProblemLists)
                    .ThenInclude(p => p.Goals)
                .FirstOrDefaultAsync(a => a.PatientId == patient.PatientId);

            var treatmentPlanViewModel = new PatientTreatmentPlanTabViewModel();

            // Only show problems coming from PsychiatricAssessment (PsyProblemList)
            var psyProblemListProblems = new List<ProblemViewModel>();
             if (assessment?.ProblemLists != null)
             {
                 foreach (var problem in assessment.ProblemLists)
                 {
                     psyProblemListProblems.Add(new ProblemViewModel
                     {
                         PsyProblemListId = problem.PsyProblemListId,
                         InitialAssessmentFormId = assessment.PsychiatricAssessmentId,
                         Problems = problem.Problem,
                         Status = problem.Status,
                         DateAdded = problem.CreatedAt,
                         Goals = problem.Goals?.Select(g => new GoalViewModel
                         {
                             GoalId = g.GoalId,
                             Description = g.Description,
                             Status = g.Status,
                             NotedBy = g.NotedBy,
                             TargetDate = g.TargetDate
                         }).ToList() ?? new List<GoalViewModel>(),
                         Interventions = new List<InterventionViewModel>()
                     });
                 }
             }

            // Show only PsyProblemList items
            var allProblems = psyProblemListProblems.ToList();
            
            foreach (var problem in allProblems)
            {
                Console.WriteLine($"Problem: {problem.Problems}, Status: {problem.Status}, DateAdded: {problem.DateAdded}");
            }

            // Declare interventionsByProblem and medsByProblem before use
            var interventionsByProblem = new Dictionary<int, List<InterventionViewModel>>();
            var medsByProblem = new Dictionary<int, List<InterventionViewModel>>();

            // Fetch interventions from the database and group by PsyProblemListId
            var interventionEntities = await _context.Interventions
                .Include(i => i.ServiceModality) // <-- Add this line
                .Where(i => i.PatientId == patient.PatientId)
                .ToListAsync();

            interventionsByProblem = interventionEntities
                .GroupBy(i => i.PsyProblemListId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(i => new InterventionViewModel
                    {
                        InterventionId = i.InterventionId,
                        ServiceModalityName = i.ServiceModality?.ServiceName,
                        Description = i.Description,
                        Frequency = i.DurationFrequency,
                        Status = i.Status,
                        NotedBy = i.NotedBy,
                        DateAdded = i.DateAdded,
                        MedicationOrderId = null,
                        MedicineId = null,
                        MedicationName = null,
                        UnitPerDose = null
                    }).ToList()
                );

            // After fetching and grouping interventions
            foreach (var kvp in interventionsByProblem)
            {
                Console.WriteLine($"ProblemListId: {kvp.Key}");
                foreach (var intervention in kvp.Value)
                {
                    Console.WriteLine($"  InterventionId:{intervention.ServiceModalityName} {intervention.InterventionId}, Description: {intervention.Description}, Status: {intervention.Status}, DateAdded: {intervention.DateAdded}");
                }
            }

            // Attach interventions and medications to each problem
            foreach (var problemVm in allProblems)
            {
                var key = problemVm.PsyProblemListId;

                if (interventionsByProblem.TryGetValue(key, out var interventionsList))
                {
                    problemVm.Interventions.AddRange(interventionsList);
                }

                if (medsByProblem.TryGetValue(key, out var medsList))
                {
                    problemVm.Interventions.AddRange(medsList);
                }

                problemVm.Interventions = problemVm.Interventions
                    .OrderByDescending(i => i.DateAdded ?? DateTime.MinValue)
                    .ToList();
            }

            treatmentPlanViewModel.Problems = allProblems;

            var meds = await _context.MedicationOrders
                .Include(m => m.Medicine)
                .Where(m => m.PatientId == patient.PatientId && m.Status != "Removed" && m.Status != "Deleted")
                .ToListAsync();

            medsByProblem = meds
                .GroupBy(m => m.PsyProblemListId ?? 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new InterventionViewModel
                    {
                        InterventionId = 0,
                        ServiceTypeName = null,
                        ServiceModalityName = null,
                        Description = string.IsNullOrWhiteSpace(m.Note) ? $"Medication order for {m.Medicine?.GenericName}" : m.Note,
                        Frequency = null,
                        Status = m.Status,
                        ScheduledType = m.ScheduledType,
                        NotedBy = m.CreatedBy,
                        DateAdded = m.CreatedAt,
                        MedicationOrderId = m.MedicationOrderId,
                        MedicineId = m.MedicineId,
                        MedicationName = m.Medicine != null
                            ? $"{m.Medicine.GenericName} ({m.Medicine.BrandName}) - {m.Medicine.Strength} {m.Medicine.Unit} {m.Medicine.Form}"
                            : null,
                        UnitPerDose = m.UnitPerDose + (m.Medicine != null ? $" {m.Medicine.Form}" : string.Empty)
                    }).ToList()
                );

            foreach (var m in meds)
            {
                Console.WriteLine($"MedicationOrderId: {m.MedicationOrderId}, PsyProblemListId: {m.PsyProblemListId}, Medicine: {m.Medicine?.GenericName}");
            }

            // After merging allProblems
            foreach (var problem in allProblems)
            {
                Console.WriteLine($"Problem: {problem.Problems}, Status: {problem.Status}, DateAdded: {problem.DateAdded}");
            }

            foreach (var problemVm in treatmentPlanViewModel.Problems)
            {
                var key = problemVm.PsyProblemListId;

                if (medsByProblem.TryGetValue(key, out var medsList))
                {
                    problemVm.Interventions.AddRange(medsList);
                }

                problemVm.Interventions = problemVm.Interventions
                    .OrderByDescending(i => i.DateAdded ?? DateTime.MinValue)
                    .ToList();
            }

            // load progress notes for this patient via interventions
            var progressNotes = await _context.ProgressNotes
                .Include(pn => pn.Intervention)
                .Where(pn => pn.Intervention != null && pn.Intervention.PatientId == patient.PatientId)
                .ToListAsync();

            // Group notes by intervention
            var notesByIntervention = progressNotes
                .GroupBy(n => n.InterventionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(n => new ProgressNoteSummaryViewModel
                    {
                        ProgressNoteId = n.ProgressNoteId,
                        CreatedAt = n.CreatedAt,
                        Clinician = n.Clinician ?? "-",
                        SoapRaw = n.SoapRaw ?? "",
                        Subjective = n.Subjective ?? "",
                        Objective = n.Objective ?? "",
                        Assessment = n.Assessment ?? "",
                        Plan = n.Plan ?? ""
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList()
                );

            // Debug output to see the grouping
            Console.WriteLine("\nNotes grouped by intervention:");
            foreach (var kvp in notesByIntervention)
            {
                Console.WriteLine($"Intervention {kvp.Key} has {kvp.Value.Count} notes");
                foreach (var note in kvp.Value)
                {
                    Console.WriteLine($"  Note {note.ProgressNoteId}: {note.CreatedAt}");
                    Console.WriteLine($"  SOAP: {note.SoapRaw}");
                }
            }

            // map interventions -> summary VM (for Progress Notes tab)
            var interventionSummaries = interventionEntities
                .Select(i => new InterventionSummaryViewModel
                {
                    InterventionId = i.InterventionId,
                    Title = i.ServiceModality?.ServiceName ?? "Intervention",
                    Description = i.Description ?? "",
                    Status = i.Status ?? "Active",
                    Clinician = i.NotedBy ?? "",
                    LastNoteDate = (notesByIntervention.TryGetValue(i.InterventionId, out var nlist) && nlist.Any())
                        ? nlist.First().CreatedAt
                        : i.DateAdded,
                    ProgressNotes = notesByIntervention.ContainsKey(i.InterventionId)
                        ? notesByIntervention[i.InterventionId]
                        : new List<ProgressNoteSummaryViewModel>()
                })
                .ToList();

            // attach progress notes to each intervention summary
            foreach (var s in interventionSummaries)
            {
                if (notesByIntervention.TryGetValue(s.InterventionId, out var noteList))
                {
                    s.ProgressNotes = noteList;
                    s.LastNoteDate = noteList.FirstOrDefault()?.CreatedAt ?? s.LastNoteDate;
                }
                else
                {
                    s.ProgressNotes = new List<ProgressNoteSummaryViewModel>();
                }
            }

            // --- NEW: attach progress notes to interventions grouped by problem (so TreatmentPlan shows notes per intervention) ---
            foreach (var kvp in interventionsByProblem)
            {
                var list = kvp.Value;
                foreach (var ivm in list)
                {
                    // safe: only set if there are notes for this intervention
                    if (ivm?.InterventionId != null && notesByIntervention.TryGetValue(ivm.InterventionId, out var pnList))
                    {
                        // try strongly-typed assignment first (common case)
                        var prop = ivm.GetType().GetProperty("ProgressNotes");
                        if (prop != null && prop.PropertyType.IsAssignableFrom(typeof(List<ProgressNoteSummaryViewModel>)))
                        {
                            prop.SetValue(ivm, pnList);
                        }

                        // update last note date if property exists
                        var lastProp = ivm.GetType().GetProperty("LastNoteDate");
                        if (lastProp != null && lastProp.PropertyType == typeof(DateTime?))
                        {
                            lastProp.SetValue(ivm, pnList.FirstOrDefault()?.CreatedAt ?? (DateTime?)ivm.DateAdded);
                        }
                    }
                    else
                    {
                        // ensure ProgressNotes exists and is an empty list to avoid nulls in views (if property present)
                        var prop = ivm?.GetType().GetProperty("ProgressNotes");
                        if (prop != null && prop.PropertyType.IsAssignableFrom(typeof(List<ProgressNoteSummaryViewModel>)))
                        {
                            prop.SetValue(ivm, new List<ProgressNoteSummaryViewModel>());
                        }
                    }
                }
            }
            // --- end new code ---
            
            var interventions = new List<Intervention>();

            // Load (or attempt) related forms
            var intakeForm = patient.IntakeForm; // already included
            var initialAssessment = await _context.InitialAssessmentForms
                .FirstOrDefaultAsync(f => f.PatientId == patient.PatientId); // adjust DbSet name if different
            var psychAssessment = await _context.PsychiatricAssessments
                .FirstOrDefaultAsync(a => a.PatientId == patient.PatientId);

            string StatusFromDates(DateTime? completedAt) => completedAt.HasValue ? "Completed" : "In Progress";

            // BUILD FORMS (skip Not Started except Intake)
            var forms = new List<ClinicalFormCardViewModel>();

            // Intake (always show – even if not started)
            forms.Add(new ClinicalFormCardViewModel
            {
                FormType = "Intake Form",
                FormId = intakeForm?.IntakeFormsId,
                Status = intakeForm == null ? "Not Started" : StatusFromDates(intakeForm.CompletedAt),
                CreatedAt = intakeForm?.CreatedAt,
                Clinician = intakeForm?.CreatedBy ?? "-",
                ActionUrl = intakeForm == null
                    ? Url.Action("CreateIntakeForm", "Intake", new { patientId = patient.PatientId })
                    : Url.Action("EditIntakeForm", "Intake", new { id = intakeForm.IntakeFormsId })
            });

            // Initial Assessment (only if exists)
            if (initialAssessment != null)
            {
                forms.Add(new ClinicalFormCardViewModel
                {
                    FormType = "Initial Assessment",
                    FormId = initialAssessment.InitialAssessmentFormId,
                    Status = StatusFromDates(initialAssessment.CompletedAt),
                    CreatedAt = initialAssessment.CreatedAt,
                    Clinician = initialAssessment.CreatedBy ?? "-",
                    ActionUrl = Url.Action("EditInitialAssessmentForm", "Assessment",
                        new { id = initialAssessment.InitialAssessmentFormId })
                });
            }

            // Psychiatric Assessment (only if exists)
            if (psychAssessment != null)
            {
                forms.Add(new ClinicalFormCardViewModel
                {
                    FormType = "Psychiatric Assessment",
                    FormId = psychAssessment.PsychiatricAssessmentId,
                    Status = psychAssessment.CreatedAt.HasValue ? "Completed" : "In Progress",
                    CreatedAt = psychAssessment.CreatedAt,
                    Clinician = psychAssessment.CreatedBy ?? "-",
                    ActionUrl = Url.Action("PsychiatricAssessmentForm", "PsychiatricAssessment",
                        new { id = psychAssessment.PsychiatricAssessmentId })
                });
            }

            var clinicalFormsVm = new PatientClinicalFormTabViewModel
            {
                PatientId = patient.PatientId,
                Forms = forms
            };

            var viewModel = new PatientProfilePageViewModel
            {
                PatientId = patient.PatientId,
                PatientRefId = patient.PatientRefId,
                AvatarUrl = patient.PhotoUrl,
                PatientName = $"{patient.Firstname} {patient.Lastname}",
                Status = patient.PatientStatus,
                OverViewTab = new PatientOverViewTabViewModel
                {
                    FoodAllergies = new List<string> { "Peanuts", "Shellfish" },
                    DrugAllergies = new List<string> { "Penicillin" },
                    ActiveMedications = new List<string> { "Aspirin", "Lisinopril" },
                    TreatmentTeams = new List<TreatmentTeamMemberViewModel>()
                },
                PersonalInfoTab = new PatientPersonalInfoTabViewModel()
                {
                    PatientRefId = patient.PatientRefId ?? "-",
                    PatientId = patient.PatientId,
                    FirstName = patient.Firstname,
                    LastName = patient.Lastname,
                    MiddleName = patient.MiddleName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = CalculateAge(patient.DateOfBirth),
                    PhotoUrl = patient.PhotoUrl,
                    MaritalStatus = patient.MaritalStatus,
                    Occupation = patient.Occupation,
                    Religion = patient.Religion,
                    Sex = patient.Sex,
                    PhoneNumber = patient.PhoneNumber,
                    Address = patient.Address,
                    FamilyConstellation = patient.IntakeForm?.FamilyMembers?
                        .Select(fm => new FamilyConstellationViewModel
                        {
                            Name = fm?.Name ?? string.Empty,
                            Relationship = fm?.Relationship ?? string.Empty,
                            Age = fm?.Age.ToString() ?? string.Empty,
                            Comments = fm?.Comments ?? string.Empty,
                        }).ToList() ?? new List<FamilyConstellationViewModel>()
                },
                MedicalHistoryTab = new PatientMedicalHistoryTabViewModel(),
                ClinicalFormTab = clinicalFormsVm,
                TreatmentPlanTab = new PatientTreatmentPlanTabViewModel
                {
                    Problems = treatmentPlanViewModel.Problems
                },
                ProgressNotesTab = new PatientProgressNotesTabViewModel()
                {
                    PatientId = patient.PatientId,
                    Interventions = interventionSummaries ?? new List<InterventionSummaryViewModel>(),
                    // Prefer selecting the first intervention that has progress notes, otherwise the first intervention
                    SelectedInterventionId = (interventionSummaries != null
                        ? interventionSummaries.FirstOrDefault(i => i.ProgressNotes != null && i.ProgressNotes.Any())?.InterventionId
                            ?? interventionSummaries.FirstOrDefault()?.InterventionId
                        : (int?)null),
                    InterventionFilter = "All",
                },
                ActivityLogTab = new PatientActivityLogTabViewModel(),
                Interventions = interventions,
            };

            // after loading patient:
            var docs = await _context.PatientDocuments
                .Where(d => d.PatientId == patient.PatientId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            viewModel.MedicalHistoryTab = new PatientMedicalHistoryTabViewModel
            {
                PatientId = patient.PatientId,
                Documents = docs,
                CanUpload = true
            };

            foreach (var intervention in interventions)
            {
                Console.WriteLine($"Intervention ID: {intervention.InterventionId}, Description: {intervention.Description}");
            }

            ViewBag.PatientId = patient.PatientId;
            ViewBag.InitialAssessmentFormId = initialAssessment?.InitialAssessmentFormId; // add this

            var serviceTypesList = await _context.ServiceTypes
                                    .Where(st => st.Status == "Active")
                                    .ToListAsync();

            ViewBag.ServiceTypes = serviceTypesList
                .Select(st => new SelectListItem
                {
                    Value = st.ServiceTypeId.ToString(),
                    Text = st.ServiceName
                })
                .ToList();

            var servicesList = await _context.Services.ToListAsync();

            ViewBag.Services = servicesList
                .Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = s.ServiceName
                })
                .ToList();

            // after patient loaded
            List<ActivityLog> initialLogs = new();
            try
            {
                initialLogs = await _context.ActivityLogs
                    .Where(l => l.PatientId == patient.PatientId)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(25)
                    .ToListAsync();
            }
            catch (SqlException)
            {
                // Table likely missing (migration not applied). Skip silently.
            }

            viewModel.ActivityLogTab = new PatientActivityLogTabViewModel
            {
                PatientId = patient.PatientId,
                ActivityLogs = initialLogs
            };


            return View(viewModel);
        }

        //Helper to calculate age
        public string CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age.ToString();
        }
        
        

        // AJAX: refresh partial
        [HttpGet]
        public async Task<IActionResult> MedicalHistoryPartial(int patientId)
        {
            var docs = await _context.PatientDocuments
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            var vm = new PatientMedicalHistoryTabViewModel
            {
                PatientId = patientId,
                Documents = docs
            };
            return PartialView("ProfileTabs/_MedicalHistory", vm);
        }

        [HttpPost]
        [RequestSizeLimit(50_000_000)] // 50 MB
        public async Task<IActionResult> UploadMedicalHistoryDocument(int patientId, IFormFile file, string? subFolder = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Empty file.");

            if (file.Length > 50_000_000)
                return BadRequest("File too large.");

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null) return NotFound("Patient not found.");

            var patientName = GetPatientFullName(patient);

            // Sanitize filename
            var originalName = Path.GetFileName(file.FileName);
            var safeName = string.Join("_", originalName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(safeName)) safeName = Guid.NewGuid().ToString("N");

            await using var stream = file.OpenReadStream();
            var url = await _cloudSvc.UploadPatientDocumentAsync(stream, safeName, patientId, subFolder);
            if (string.IsNullOrWhiteSpace(url)) return StatusCode(500, "Upload failed.");

            var doc = new PatientDocument
            {
                PatientId = patientId,
                FileName = safeName,
                Url = url,
                ContentType = file.ContentType,
                UploadedBy = User?.Identity?.Name ?? "System"
            };
            _context.PatientDocuments.Add(doc);
            await _context.SaveChangesAsync();

            await _activityService.LogAsync(
                User?.Identity?.Name ?? "System",
                "Uploaded document",
                $"File: {safeName} for {patientName}",
                category: "MedicalHistory",
                severity: "Info",
                patientId: patientId);

            await _activityService.NotifyAsync(
                User?.Identity?.Name ?? "System",
                $"Document '{safeName}' uploaded for {patientName}",
                type: "Success");

            return Ok(new
            {
                doc.PatientDocumentId,
                doc.FileName,
                doc.Url,
                doc.ContentType,
                OriginalFileName = originalName,
                UploadedAt = doc.UploadedAt.ToString("u")
            });
        }

        // Example logging (place in actions where changes occur)
        private async Task LogPatientAsync(int patientId, string action, string? desc = null, string category = "Profile")
        {
            var name = await _context.Patients
                .Where(p => p.PatientId == patientId)
                .Select(p => p.Firstname + " " + p.Lastname)
                .FirstOrDefaultAsync() ?? $"Patient {patientId}";

            var fullDesc = string.IsNullOrWhiteSpace(desc) ? $"For {name}" : $"{desc} (For {name})";
            await _activityService.LogAsync(User?.Identity?.Name ?? "System", action, fullDesc, category, "Info", patientId);
        }

        // Activity logs partial (AJAX)
        [HttpGet]
        public async Task<IActionResult> ActivityLogs(int patientId, int page = 1, string? search = null, string? category = null)
        {
            var logs = await _activityService.GetPatientLogsAsync(patientId, page, 30, search, category);
            var vm = new PatientActivityLogTabViewModel
            {
                PatientId = patientId,
                ActivityLogs = logs
            };
            return PartialView("ProfileTabs/_ActivityLogsTab", vm);
        }

        // Notifications endpoints (optional)
        [HttpGet]
        public async Task<IActionResult> UnreadNotifications()
        {
            try
            {
                var list = await _activityService.GetUnreadAsync(User?.Identity?.Name ?? "-");
                return Json(list.Select(n => new {
                    n.NotificationId,
                    n.Message,
                    n.Type,
                    CreatedAt = n.CreatedAt.ToString("u"),
                    n.LinkUrl
                }));
            }
            catch (SqlException)
            {
                // Notifications table not yet created
                return Json(Array.Empty<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            await _activityService.MarkReadAsync(id, User?.Identity?.Name ?? "-");
            return Ok();
        }

        // Medical History Main Tab
        [HttpGet]
        public async Task<IActionResult> MedicalHistoryMainTab(int patientId, int initialAssessmentFormId)
         {
            var ia = await _context.InitialAssessmentForms
                .Include(f => f.Patient)
                .Include(f => f.HistoryPresent)
                .Include(f => f.MedicalHistory)
                .Include(f => f.DrugUses)
                .Include(f => f.MedicalAllergies)
                .Include(f => f.SurgicalHistories)
                .Include(f => f.PhysicalExam)
                .Include(f => f.Diagnosis).ThenInclude(d => d.SubstanceUseEntries)
                .Include(f => f.Problems).ThenInclude(p => p.Goals)
                .Include(f => f.Recommendation)
                .Include(f => f.MentalStatusExamination)
                .FirstOrDefaultAsync(f => f.PatientId == patientId && f.InitialAssessmentFormId == initialAssessmentFormId);

            if (ia == null)
            {
                TempData["ErrorMessage"] = "No medical history found for this patient.";
                return PartialView("ProfileTabs/MedicalHistoryTab/_MedicalHistoryMainTab", new AssessmentFormViewModel());
            }

            var vm = new AssessmentFormViewModel
            {
                PatientId = ia.PatientId,
                AssessmentId = ia.InitialAssessmentFormId,

                HistoryPresent = ia.HistoryPresent == null ? null : new HistoryPresentViewModel
                {
                    HistoryPresentId = ia.HistoryPresent.HistoryPresentId,
                    OnsetOfDrugUse = ia.HistoryPresent.OnsetOfDrugUse,
                    ReasonForFirstUse = ia.HistoryPresent.ReasonForFirstUse,
                    HistoryOfImprisonment = ia.HistoryPresent.HistoryOfImprisonment,
                    PreviousDrugRehab = ia.HistoryPresent.PreviousDrugRehab,
                    WhoInvitedFirstUse = ia.HistoryPresent.WhoInvitedFirstUse,
                    NumberOfPeopleFirstUse = ia.HistoryPresent.NumberOfPeopleFirstUse,
                    LastUseOfSubstance = ia.HistoryPresent.LastUseOfSubstance,
                    AmountConsumedFirstUse = ia.HistoryPresent.AmountConsumedFirstUse,
                },

                MedicalHistory = ia.MedicalHistory == null ? null : new MedicalHistoryViewModel
                {
                    IsHypertensive = ia.MedicalHistory.IsHypertensive,
                    IsDiabetic = ia.MedicalHistory.IsDiabetic,
                    IsAsthmatic = ia.MedicalHistory.IsAsthmatic,
                    OtherConditions = ia.MedicalHistory.OtherConditions,
                    MaternalHypertension = ia.MedicalHistory.MaternalHypertension,
                    MaternalDiabetic = ia.MedicalHistory.MaternalDiabetic,
                    MaternalNone = ia.MedicalHistory.MaternalNone,
                    PaternalHypertension = ia.MedicalHistory.PaternalHypertension,
                    PaternalDiabetic = ia.MedicalHistory.PaternalDiabetic,
                    PaternalNone = ia.MedicalHistory.PaternalNone,

                    // 🥗 Map Food Allergies
                    FoodAllergies = ia.MedicalAllergies?
                        .Where(a => a.AllergyType == "Food")
                        .Select(a => a.AllergyName)
                        .ToList() ?? new List<string>(),

                    // 💊 Map Drug Allergies
                    DrugAllergies = ia.MedicalAllergies?
                        .Where(a => a.AllergyType == "Drug")
                        .Select(a => a.AllergyName)
                        .ToList() ?? new List<string>(),

                    // 🏥 Map Surgical Operations
                    SurgicalOperations = ia.SurgicalHistories?
                        .Select(s => new SurgicalOperation
                        {
                            Year = s.Year,
                            Duration = s.Duration,
                            Hospital = s.Hospital,
                            Operation = s.Operation
                        }).ToList() ?? new List<SurgicalOperation>()
                },


                DrugUseHistory = new DrugUseHistoryViewModel
                {
                    DrugUseEntries = ia.DrugUses?.Select(d => new DrugUseEntry
                    {
                        DrugHistoryId = d.DrugUseId,
                        AssessmentFormId = d.InitialAssessmentFormId,
                        SubstanceName = d.SubstanceName,
                        Route = d.Route,
                        QuantityPerDay = d.QuantityPerDay,
                        Frequency = d.Frequency,
                        FirstUse = d.FirstUse,
                        EffectsWhenHigh = d.EffectsWhenHigh,
                        EffectsWhenWanes = d.EffectsWhenWanes,
                        CreatedAt = d.CreatedAt,
                        CreatedBy = d.CreatedBy,
                        UpdatedAt = d.UpdatedAt,
                        UpdatedBy = d.UpdatedBy
                    }).ToList() ?? new List<DrugUseEntry>()
                },
            };

            return PartialView("ProfileTabs/MedicalHistoryTab/_MedicalHistoryMainTab", vm);
        }

        [HttpGet]
        public async Task<IActionResult> OverviewTab(int patientId)
        {
            if (patientId <= 0) return BadRequest("Invalid patientId.");

            var vm = new PatientOverViewTabViewModel
            {
                PatientId = patientId
            };

            // Clinical team
            vm.TreatmentTeams = await _context.ClinicalStaffPatients
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .Include(x => x.ClinicalStaff)
                .Select(x => new TreatmentTeamMemberViewModel
                {
                    ClinicalStaffId = x.ClinicalStaffId,
                    Firstname = x.ClinicalStaff.Firstname,  // adjust property names if different
                    Lastname = x.ClinicalStaff.Lastname,
                    Position = x.ClinicalStaff.Position,
                    // AvatarUrl = x.ClinicalStaff.ImageUrl
                })
                .ToListAsync();

            // Allergies from InitialAssessmentForm.MedicalAllergies
            var iaf = await _context.InitialAssessmentForms
                .AsNoTracking()
                .Include(i => i.MedicalAllergies)
                .FirstOrDefaultAsync(i => i.PatientId == patientId);

            if (iaf?.MedicalAllergies != null)
            {
                vm.FoodAllergies = iaf.MedicalAllergies
                    .Where(a => a.AllergyType == "Food" && !string.IsNullOrWhiteSpace(a.AllergyName))
                    .Select(a => a.AllergyName!.Trim())
                    .Distinct()
                    .ToList();

                vm.DrugAllergies = iaf.MedicalAllergies
                    .Where(a => a.AllergyType == "Drug" && !string.IsNullOrWhiteSpace(a.AllergyName))
                    .Select(a => a.AllergyName!.Trim())
                    .Distinct()
                    .ToList();
            }

            // Optionally populate today's notes and active meds here if needed

            return PartialView("ProfileTabs/_OverviewTab", vm);
        }
        
         [HttpPost]
        public async Task<IActionResult> SaveHistoryOfPresentIllnessProfile(AssessmentFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid || model.PatientId == null)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                    return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
                }

                // Find the patient
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == model.PatientId);

                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
                }

                // Get or create assessment form
                var assessmentForm = await _context.InitialAssessmentForms
                    .Include(iaf => iaf.HistoryPresent)
                    .FirstOrDefaultAsync(iaf => iaf.PatientId == model.PatientId);

                if (assessmentForm == null)
                {
                    assessmentForm = new InitialAssessmentForm
                    {
                        PatientId = patient.PatientId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity.Name ?? "System"
                    };
                    _context.InitialAssessmentForms.Add(assessmentForm);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    assessmentForm.UpdatedAt = DateTime.Now;
                    assessmentForm.UpdatedBy = User.Identity.Name ?? "System";
                }

                // Handle History Present section
                if (model.HistoryPresent != null)
                {
                    if (assessmentForm.HistoryPresent == null)
                    {
                        assessmentForm.HistoryPresent = new HistoryPresent
                        {
                            InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = User.Identity.Name ?? "System"
                        };
                        _context.HistoryPresents.Add(assessmentForm.HistoryPresent);
                    }
                    else
                    {
                        assessmentForm.HistoryPresent.UpdatedAt = DateTime.Now;
                        assessmentForm.HistoryPresent.UpdatedBy = User.Identity.Name ?? "System";
                    }

                    // Map ViewModel to Model
                    assessmentForm.HistoryPresent.OnsetOfDrugUse = model.HistoryPresent.OnsetOfDrugUse ?? string.Empty;
                    assessmentForm.HistoryPresent.ReasonForFirstUse = model.HistoryPresent.ReasonForFirstUse ?? string.Empty;
                    assessmentForm.HistoryPresent.HistoryOfImprisonment = model.HistoryPresent.HistoryOfImprisonment ?? string.Empty;
                    assessmentForm.HistoryPresent.PreviousDrugRehab = model.HistoryPresent.PreviousDrugRehab ?? string.Empty;
                    assessmentForm.HistoryPresent.WhoInvitedFirstUse = model.HistoryPresent.WhoInvitedFirstUse ?? string.Empty;
                    assessmentForm.HistoryPresent.NumberOfPeopleFirstUse = model.HistoryPresent.NumberOfPeopleFirstUse;
                    assessmentForm.HistoryPresent.LastUseOfSubstance = model.HistoryPresent.LastUseOfSubstance ?? string.Empty;
                    assessmentForm.HistoryPresent.AmountConsumedFirstUse = model.HistoryPresent.AmountConsumedFirstUse ?? string.Empty;
                }

                // Update patient status
                await EnsurePatientStatusOnAssessment(patient.PatientId);

                await _context.SaveChangesAsync();

                // --- activity log & notification (History Present) ---
                var user = User?.Identity?.Name ?? "System";
                var pat = await _context.Patients.FindAsync(model.PatientId);
                await _activityService.LogAsync(user,
                    "Saved History of Present Illness",
                    $"Updated HPI for {GetPatientFullName(pat)}",
                    "Assessment",
                    "Info",
                    pat?.PatientId);
                await _activityService.NotifyAsync(user,
                    $"HPI saved for {GetPatientFullName(pat)}",
                    type: "Success");
                // --- end ---

                TempData["SuccessMessage"] = "Initial assessment form has been saved successfully.";
                // FIX: Redirect to Index with patientId only
                return RedirectToAction("Index", new { id = model.PatientId });
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = "Database error occurred while saving the assessment.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the assessment.";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveDrugHistoryProfile(AssessmentFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                    return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
                }

                // Get or create assessment form
                var assessmentForm = await _context.InitialAssessmentForms
                    .Include(iaf => iaf.DrugUses)
                    .FirstOrDefaultAsync(iaf => iaf.PatientId == model.PatientId);

                if (assessmentForm == null)
                {
                    assessmentForm = new InitialAssessmentForm
                    {
                        PatientId = model.PatientId.Value,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System",
                        DrugUses = new List<DrugUse>()
                    };
                    _context.InitialAssessmentForms.Add(assessmentForm);
                }

                // Update or add drug use entries
                if (model.DrugUseHistory?.DrugUseEntries != null)
                {
                    var existingUses = assessmentForm.DrugUses?.ToList() ?? new List<DrugUse>();

                    // Remove deleted ones
                    foreach (var existing in existingUses)
                    {
                        if (!model.DrugUseHistory.DrugUseEntries.Any(e => e.SubstanceName == existing.SubstanceName))
                        {
                            _context.DrugUses.Remove(existing);
                        }
                    }

                    // Add or update entries
                    foreach (var entry in model.DrugUseHistory.DrugUseEntries)
                    {
                        var existing = existingUses.FirstOrDefault(e => e.SubstanceName == entry.SubstanceName);
                        if (existing != null)
                        {
                            existing.Route = entry.Route;
                            existing.QuantityPerDay = entry.QuantityPerDay;
                            existing.Frequency = entry.Frequency;
                            existing.FirstUse = entry.FirstUse;
                            existing.EffectsWhenHigh = entry.EffectsWhenHigh;
                            existing.EffectsWhenWanes = entry.EffectsWhenWanes;
                            existing.UpdatedAt = DateTime.Now;
                            existing.UpdatedBy = User.Identity?.Name ?? "System";
                        }
                        else
                        {
                            assessmentForm.DrugUses.Add(new DrugUse
                            {
                                SubstanceName = entry.SubstanceName,
                                Route = entry.Route,
                                QuantityPerDay = entry.QuantityPerDay,
                                Frequency = entry.Frequency,
                                FirstUse = entry.FirstUse,
                                EffectsWhenHigh = entry.EffectsWhenHigh,
                                EffectsWhenWanes = entry.EffectsWhenWanes,
                                CreatedAt = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            });
                        }
                    }
                }


                // Update patient status
                await EnsurePatientStatusOnAssessment(model.PatientId);

                await _context.SaveChangesAsync();

                // --- log ---
                var user = User?.Identity?.Name ?? "System";
                var pat = await _context.Patients.FindAsync(model.PatientId);
                await _activityService.LogAsync(user,
                    "Saved Drug Use History",
                    $"Updated drug use history for {GetPatientFullName(pat)}",
                    "Assessment",
                    "Info",
                    pat?.PatientId);
                await _activityService.NotifyAsync(user,
                    $"Drug history saved for {GetPatientFullName(pat)}",
                    type: "Success");
                // --- end ---

                TempData["SuccessMessage"] = "Drug history has been saved successfully.";
                return RedirectToAction("Index", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the drug history.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveMedicalHistoryProfile(AssessmentFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                    return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
                }

                // Get or create assessment form
                var assessmentForm = await _context.InitialAssessmentForms
                    .Include(iaf => iaf.MedicalHistory)
                    .Include(iaf => iaf.MedicalAllergies)
                    .Include(iaf => iaf.SurgicalHistories)
                    .FirstOrDefaultAsync(iaf => iaf.PatientId == model.PatientId);

                if (assessmentForm == null)
                {
                    assessmentForm = new InitialAssessmentForm
                    {
                        PatientId = model.PatientId.Value,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                    _context.InitialAssessmentForms.Add(assessmentForm);
                    await _context.SaveChangesAsync(); // Save to get ID
                }

                // Update or create medical history
                if (assessmentForm.MedicalHistory == null)
                {
                    assessmentForm.MedicalHistory = new MedicalHistory
                    {
                        InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                }

                // Update medical conditions
                assessmentForm.MedicalHistory.IsHypertensive = model.MedicalHistory.IsHypertensive;
                assessmentForm.MedicalHistory.IsDiabetic = model.MedicalHistory.IsDiabetic;
                assessmentForm.MedicalHistory.IsAsthmatic = model.MedicalHistory.IsAsthmatic;
                assessmentForm.MedicalHistory.OtherConditions = model.MedicalHistory.OtherConditions;

                // Update heredofamilial diseases
                assessmentForm.MedicalHistory.MaternalHypertension = model.MedicalHistory.MaternalHypertension;
                assessmentForm.MedicalHistory.MaternalDiabetic = model.MedicalHistory.MaternalDiabetic;
                assessmentForm.MedicalHistory.MaternalNone = model.MedicalHistory.MaternalNone;
                assessmentForm.MedicalHistory.PaternalHypertension = model.MedicalHistory.PaternalHypertension;
                assessmentForm.MedicalHistory.PaternalDiabetic = model.MedicalHistory.PaternalDiabetic;
                assessmentForm.MedicalHistory.PaternalNone = model.MedicalHistory.PaternalNone;

                // Update allergies
                if (assessmentForm.MedicalAllergies != null)
                {
                    _context.MedicalAllergies.RemoveRange(assessmentForm.MedicalAllergies);
                }

                // Add food allergies
                if (model.MedicalHistory.FoodAllergies != null)
                {
                    foreach (var allergy in model.MedicalHistory.FoodAllergies)
                    {
                        if (!string.IsNullOrWhiteSpace(allergy))
                        {
                            assessmentForm.MedicalAllergies.Add(new MedicalAllergy
                            {
                                AllergyType = "Food",
                                AllergyName = allergy,
                                CreatedAt = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            });
                        }
                    }
                }

                // Add drug allergies
                if (model.MedicalHistory.DrugAllergies != null)
                {
                    foreach (var allergy in model.MedicalHistory.DrugAllergies)
                    {
                        if (!string.IsNullOrWhiteSpace(allergy))
                        {
                            assessmentForm.MedicalAllergies.Add(new MedicalAllergy
                            {
                                AllergyType = "Drug",
                                AllergyName = allergy,
                                CreatedAt = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            });
                        }
                    }
                }

                // Update surgical operations
                if (assessmentForm.SurgicalHistories != null)
                {
                    _context.SurgicalHistories.RemoveRange(assessmentForm.SurgicalHistories);
                }

                if (model.MedicalHistory.SurgicalOperations != null)
                {
                    foreach (var operation in model.MedicalHistory.SurgicalOperations)
                    {
                        if (!string.IsNullOrWhiteSpace(operation.Operation))
                        {
                            assessmentForm.SurgicalHistories.Add(new SurgicalHistory
                            {
                                Year = operation.Year,
                                Duration = operation.Duration,
                                Hospital = operation.Hospital,
                                Operation = operation.Operation,
                                CreatedAt = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            });
                        }
                    }
                }

                // Update patient status
                await EnsurePatientStatusOnAssessment(model.PatientId);

                await _context.SaveChangesAsync();

                // --- log ---
                var user = User?.Identity?.Name ?? "System";
                var pat = await _context.Patients.FindAsync(model.PatientId);
                await _activityService.LogAsync(user,
                    "Saved Medical History",
                    $"Updated medical history for {GetPatientFullName(pat)}",
                    "Assessment",
                    "Info",
                    pat?.PatientId);
                await _activityService.NotifyAsync(user,
                    $"Medical history saved for {GetPatientFullName(pat)}",
                    type: "Success");
                // --- end ---

                TempData["SuccessMessage"] = "Medical history has been saved successfully.";
                return RedirectToAction("Index", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the medical history.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }
        

           private async Task EnsurePatientStatusOnAssessment(int? patientId)
        {
            if (!patientId.HasValue) return;

            var patient = await _context.Patients.FindAsync(patientId.Value);
            if (patient == null) return;

            var Pending = PatientStatusEnum.PendingAssessment.ToString();
            var onAssessment = PatientStatusEnum.OnAssessment.ToString();

            if (patient.PatientStatus == Pending)
            {
                patient.PatientStatus = onAssessment;
            }
            // if already onAssessment -> keep it
            // do not override PendingApproval, Admitted, etc.

            await _context.SaveChangesAsync();
        }
    }
}



