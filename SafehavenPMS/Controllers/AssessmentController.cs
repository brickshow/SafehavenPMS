using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.ViewModel.Assessment;
using SafehavenPMS.ViewModel.Assessment.SafehavenPMS.ViewModel.Assessment;


namespace SafehavenPMS.Controllers
{
    public class AssessmentController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public AssessmentController(SafehavenPMSContext context)
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
                .Include(a => a.NewAppointments)
                .Include(c => c.ClinicalStaffPatients)
                    .ThenInclude(csp => csp.ClinicalStaff)
                .Include(c => c.InitialAssessmentForms)
                .AsQueryable();

            // Get pending assessment count (patients with Pending status)
            ViewBag.PendingAssessment = await _context.Patients
                .CountAsync(p => p.PatientStatus == PatientStatusEnum.PendingAssessment.ToString());


            // Pass current filters/sorting to view
            ViewBag.CurrentPage = page ?? 1;
            ViewBag.PageSize = pageSize ?? 10;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;
            ViewBag.SortOrder = string.IsNullOrEmpty(sortOrder) ? "descending" : sortOrder;

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Firstname.ToLower().Contains(searchQuery) ||
                    p.Lastname.ToLower().Contains(searchQuery) ||
                    p.PatientId.ToString().Contains(searchQuery));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.PatientStatus == status);
            }

            // Apply sorting
            query = sortOrder == "ascending"
                ? query.OrderBy(p => p.Firstname).ThenBy(p => p.Lastname)
                : query.OrderByDescending(p => p.CreatedAt);

            // Get total count for pagination
            ViewBag.TotalPatientCount = await query.CountAsync();

            // Apply pagination
            int totalItems = await query.CountAsync();
            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize.Value) : 1;
            ViewBag.TotalPages = totalPages;

            int currentPage = Math.Max(1, Math.Min(page ?? 1, totalPages));
            ViewBag.CurrentPage = currentPage;

            var patientList = await query
                .Skip(pageSize > 0 ? (currentPage - 1) * pageSize.Value : 0)
                .Take(pageSize > 0 ? pageSize.Value : totalItems)
                .ToListAsync();

            // Map to view model - first get the data, then project in memory
            var patientsData = await query.ToListAsync();
            var pendingAssessment = patientList
            .Where(p => p.PatientStatus == PatientStatusEnum.PendingAssessment.ToString() ||
                        p.PatientStatus == PatientStatusEnum.InProgress.ToString() ||
                        p.PatientStatus == PatientStatusEnum.PendingApproval.ToString())
            .Select(p =>
            {
                var appointment = p.NewAppointments.FirstOrDefault();
                var physician = p.ClinicalStaffPatients.FirstOrDefault(csp => csp.ClinicalStaff.Position == "Physician")?.ClinicalStaff;
                
                // Get the latest assessment with CompletedAt
                var latestAssessment = p.InitialAssessmentForms?
                    .OrderByDescending(f => f.CompletedAt)
                    .FirstOrDefault(f => f.CompletedAt.HasValue);

                return new PendingAssessmentViewModel
                {
                    PatientId = p.PatientId,
                    PhysicianId = appointment?.ClinicalStaffID ?? 0,
                    PhysicianName = physician != null ? $"{physician.Firstname} {physician.Lastname}" : "-",
                    Type = appointment?.Type ?? "-",
                    PatientName = $"{p.Firstname} {p.Lastname}",
                    Date = appointment?.ScheduleDate,
                    Time = appointment?.ScheduleTime,
                    CompletedDate = latestAssessment?.CompletedAt,  // This will be null if no completed assessment exists
                    Status = p.PatientStatus ?? "-"
                };
            }).ToList();

            return View(pendingAssessment);
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
        public async Task<IActionResult> EditInitialAssessmentForm(int? id)
        {
            // Check if patient ID is provided
            if (id == null)
            {
                // Redirect to Index if no ID provided
                return RedirectToAction("Index");
            }

            // Fetch patient data from database with related intake form
            // Update the patient query at the start of the action
            var patient = await _context.Patients
                .Include(p => p.IntakeForm)
                .Include(p => p.InitialAssessmentForms)
                    .ThenInclude(iaf => iaf.Diagnosis)
                        .ThenInclude(d => d.SubstanceUseEntries)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            // Check if patient exists
            if (patient == null)
            {
                // Redirect to Index if patient not found
                return RedirectToAction("Index");
            }

            // Calculate patient's current age
            var age = DateTime.Today.Year - patient.DateOfBirth.Year;
            // Adjust age if birthday hasn't occurred this year
            if (patient.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

            // Create view model with patient data
            var viewModel = new AssessmentFormViewModel
            {
                // Patient Information
                PatientId = patient.PatientId,
                FullName = $"{patient.Firstname} {patient.Lastname}",
                Age = age,
                Sex = patient.Sex ?? "-",
                Occupation = patient.Occupation ?? "-",
                Address = patient.Address ?? "-",

                // Populate History Present data
                HistoryPresent = await _context.InitialAssessmentForms
                        .Where(iaf => iaf.PatientId == id)
                        .Select(iaf => new HistoryPresentViewModel
                        {
                            HistoryPresentId = iaf.HistoryPresent.HistoryPresentId,
                            OnsetOfDrugUse = iaf.HistoryPresent.OnsetOfDrugUse,
                            ReasonForFirstUse = iaf.HistoryPresent.ReasonForFirstUse,
                            HistoryOfImprisonment = iaf.HistoryPresent.HistoryOfImprisonment,
                            PreviousDrugRehab = iaf.HistoryPresent.PreviousDrugRehab,
                            WhoInvitedFirstUse = iaf.HistoryPresent.WhoInvitedFirstUse,
                            NumberOfPeopleFirstUse = iaf.HistoryPresent.NumberOfPeopleFirstUse,
                            LastUseOfSubstance = iaf.HistoryPresent.LastUseOfSubstance,
                            AmountConsumedFirstUse = iaf.HistoryPresent.AmountConsumedFirstUse
                        })
                        .FirstOrDefaultAsync() ?? new HistoryPresentViewModel(),
                // In the EditInitialAssessmentForm action, add this to your viewModel population:

                // Populate Drug History data
                DrugUseHistory = await _context.InitialAssessmentForms
                .Where(iaf => iaf.PatientId == id)
                .Select(iaf => new DrugUseHistoryViewModel
                {
                    DrugUseEntries = iaf.DrugUses.Select(du => new DrugUseEntry
                    {
                        DrugHistoryId = du.DrugUseId,
                        SubstanceName = du.SubstanceName,
                        Route = du.Route,
                        QuantityPerDay = du.QuantityPerDay,
                        Frequency = du.Frequency,
                        FirstUse = du.FirstUse,
                        EffectsWhenHigh = du.EffectsWhenHigh,
                        EffectsWhenWanes = du.EffectsWhenWanes
                    }).ToList()
                }).FirstOrDefaultAsync() ?? new DrugUseHistoryViewModel(),

                // Populate Medical History data
                MedicalHistory = await _context.InitialAssessmentForms
                .Where(iaf => iaf.PatientId == id)
                .Select(iaf => new MedicalHistoryViewModel
                {
                    // Medical conditions - check if MedicalHistory exists
                    IsHypertensive = iaf.MedicalHistory != null && iaf.MedicalHistory.IsHypertensive,
                    IsDiabetic = iaf.MedicalHistory != null && iaf.MedicalHistory.IsDiabetic,
                    IsAsthmatic = iaf.MedicalHistory != null && iaf.MedicalHistory.IsAsthmatic,
                    OtherConditions = iaf.MedicalHistory != null ? iaf.MedicalHistory.OtherConditions : null,

                    // Heredofamilial diseases
                    MaternalHypertension = iaf.MedicalHistory != null && iaf.MedicalHistory.MaternalHypertension,
                    MaternalDiabetic = iaf.MedicalHistory != null && iaf.MedicalHistory.MaternalDiabetic,
                    MaternalNone = iaf.MedicalHistory != null && iaf.MedicalHistory.MaternalNone,
                    PaternalHypertension = iaf.MedicalHistory != null && iaf.MedicalHistory.PaternalHypertension,
                    PaternalDiabetic = iaf.MedicalHistory != null && iaf.MedicalHistory.PaternalDiabetic,
                    PaternalNone = iaf.MedicalHistory != null && iaf.MedicalHistory.PaternalNone,

                    // Allergies - check if collections exist
                    FoodAllergies = iaf.MedicalAllergies != null ?
                        iaf.MedicalAllergies
                            .Where(a => a.AllergyType == "Food")
                            .Select(a => a.AllergyName)
                            .ToList() : new List<string>(),

                    DrugAllergies = iaf.MedicalAllergies != null ?
                        iaf.MedicalAllergies
                            .Where(a => a.AllergyType == "Drug")
                            .Select(a => a.AllergyName)
                            .ToList() : new List<string>(),

                    // Surgical operations
                    SurgicalOperations = iaf.SurgicalHistories != null ?
                        iaf.SurgicalHistories
                            .Select(sh => new SurgicalOperation
                            {
                                Year = sh.Year,
                                Duration = sh.Duration,
                                Hospital = sh.Hospital,
                                Operation = sh.Operation
                            })
                            .ToList() : new List<SurgicalOperation>()
                })
                .FirstOrDefaultAsync() ?? new MedicalHistoryViewModel(),

                PhysicalExam = await _context.InitialAssessmentForms
                    .Where(iaf => iaf.PatientId == id)
                    .Select(iaf => new PhysicalExamViewModel
                    {
                        // Vital Signs
                        BP = iaf.PhysicalExam != null ? iaf.PhysicalExam.BP : null,
                        HR = iaf.PhysicalExam != null ? iaf.PhysicalExam.HR : null,
                        RR = iaf.PhysicalExam != null ? iaf.PhysicalExam.RR : null,
                        Temperature = iaf.PhysicalExam != null ? iaf.PhysicalExam.Temperature : null,
                        O2 = iaf.PhysicalExam != null ? iaf.PhysicalExam.O2 : null,

                        // System Examination
                        SkinNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.SkinNormal,
                        SkinFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.SkinFindings : null,

                        ENTNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.ENTNormal,
                        ENTFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.ENTFindings : null,

                        ChestNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.ChestNormal,
                        ChestFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.ChestFindings : null,

                        LungsNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.LungsNormal,
                        LungsFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.LungsFindings : null,

                        CVSNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.CVSNormal,
                        CVSFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.CVSFindings : null,

                        AbdomenNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.AbdomenNormal,
                        AbdomenFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.AbdomenFindings : null,

                        GUTNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.GUTNormal,
                        GUTFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.GUTFindings : null,

                        ExtremitiesNormal = iaf.PhysicalExam != null && iaf.PhysicalExam.ExtremitiesNormal,
                        ExtremitiesFindings = iaf.PhysicalExam != null ? iaf.PhysicalExam.ExtremitiesFindings : null
                    })
                    .FirstOrDefaultAsync() ?? new PhysicalExamViewModel(),

                Diagnosis = await _context.InitialAssessmentForms
                    .Where(iaf => iaf.PatientId == id)
                    .Select(iaf => new DiagnosisViewModel
                    {
                        SubstanceUses = iaf.Diagnosis != null && iaf.Diagnosis.SubstanceUseEntries != null ?
                            iaf.Diagnosis.SubstanceUseEntries
                                .Select(su => new SubstanceUseViewModel
                                {
                                    SubstanceName = su.SubstanceName ?? string.Empty,
                                    Severity = su.Severity ?? string.Empty
                                })
                                .ToList()
                            : new List<SubstanceUseViewModel>()
                    })
                    .FirstOrDefaultAsync() ?? new DiagnosisViewModel(),

                // Add this after the Diagnosis population in the viewModel initialization
                ProblemList = await _context.InitialAssessmentForms
                    .Where(iaf => iaf.PatientId == id)
                    .Select(iaf => new ProblemListViewModel
                    {
                        Problems = iaf.Problems != null && iaf.Problems.Any()
                            ? iaf.Problems.Select(p => p.Problem).ToList()
                            : new List<string>()
                    })
                    .FirstOrDefaultAsync() ?? new ProblemListViewModel(),

                Recommendation = await _context.InitialAssessmentForms
                .Where(iaf => iaf.PatientId == id)
                .Select(iaf => iaf.Recommendation != null
                    ? new RecommendationViewModel
                    {
                        ProgramType = iaf.Recommendation.ProgramType,
                        ExpectedDuration = iaf.Recommendation.ExpectedDuration,
                        Reason = iaf.Recommendation.Reason
                    }
                    : new RecommendationViewModel())
                .FirstOrDefaultAsync() ?? new RecommendationViewModel(),

                // Populate Mental Status Examination for display
                MentalStatusExamination = await _context.InitialAssessmentForms
                    .Where(iaf => iaf.PatientId == id && iaf.MentalStatusExamination != null)
                    .OrderByDescending(iaf => iaf.MentalStatusExamination.UpdatedAt ?? iaf.MentalStatusExamination.CreatedAt)
                    .Select(iaf => new MentalStatusExaminationViewModel
                    {
                        GeneralAppearanceNeat = iaf.MentalStatusExamination.GeneralAppearanceNeat,
                        GeneralAppearanceDishevelled = iaf.MentalStatusExamination.GeneralAppearanceDishevelled,
                        GeneralAppearanceInappropriate = iaf.MentalStatusExamination.GeneralAppearanceInappropriate,
                        GeneralAppearanceOthers = iaf.MentalStatusExamination.GeneralAppearanceOthers,

                        SpeechNormal = iaf.MentalStatusExamination.SpeechNormal,
                        SpeechRapid = iaf.MentalStatusExamination.SpeechRapid,
                        SpeechSlow = iaf.MentalStatusExamination.SpeechSlow,
                        SpeechIncoherent = iaf.MentalStatusExamination.SpeechIncoherent,
                        SpeechOthers = iaf.MentalStatusExamination.SpeechOthers,

                        BehaviorRelaxed = iaf.MentalStatusExamination.BehaviorRelaxed,
                        BehaviorCooperative = iaf.MentalStatusExamination.BehaviorCooperative,
                        BehaviorSuspicious = iaf.MentalStatusExamination.BehaviorSuspicious,
                        BehaviorPreoccupied = iaf.MentalStatusExamination.BehaviorPreoccupied,
                        BehaviorOthers = iaf.MentalStatusExamination.BehaviorOthers,

                        ViolenceRelaxed = iaf.MentalStatusExamination.ViolenceRelaxed,
                        ViolenceRestless = iaf.MentalStatusExamination.ViolenceRestless,
                        ViolenceClenchedFist = iaf.MentalStatusExamination.ViolenceClenchedFist,
                        ViolenceRaisedVoice = iaf.MentalStatusExamination.ViolenceRaisedVoice,
                        ViolenceOthers = iaf.MentalStatusExamination.ViolenceOthers,

                        MoodSad = iaf.MentalStatusExamination.MoodSad,
                        MoodAnxious = iaf.MentalStatusExamination.MoodAnxious,
                        MoodHappy = iaf.MentalStatusExamination.MoodHappy,
                        MoodFearful = iaf.MentalStatusExamination.MoodFearful,
                        MoodHelpless = iaf.MentalStatusExamination.MoodHelpless,
                        MoodHopeless = iaf.MentalStatusExamination.MoodHopeless,
                        MoodAngry = iaf.MentalStatusExamination.MoodAngry,
                        MoodOthers = iaf.MentalStatusExamination.MoodOthers,

                        AffectAppropriate = iaf.MentalStatusExamination.AffectAppropriate,
                        AffectInappropriate = iaf.MentalStatusExamination.AffectInappropriate,
                        AffectFlat = iaf.MentalStatusExamination.AffectFlat,
                        AffectBlunted = iaf.MentalStatusExamination.AffectBlunted,
                        AffectOthers = iaf.MentalStatusExamination.AffectOthers,

                        ThoughtsNormal = iaf.MentalStatusExamination.ThoughtsNormal,
                        ThoughtsFlightOfIdeas = iaf.MentalStatusExamination.ThoughtsFlightOfIdeas,
                        ThoughtsPreoccupied = iaf.MentalStatusExamination.ThoughtsPreoccupied,
                        ThoughtsOthers = iaf.MentalStatusExamination.ThoughtsOthers,

                        CognitionConscious = iaf.MentalStatusExamination.CognitionConscious,
                        CognitionConfused = iaf.MentalStatusExamination.CognitionConfused,
                        CognitionDrowsy = iaf.MentalStatusExamination.CognitionDrowsy,
                        CognitionOthers = iaf.MentalStatusExamination.CognitionOthers,

                        PerceptionsIllusions = iaf.MentalStatusExamination.PerceptionsIllusions,
                        PerceptionsAuditoryHallucinations = iaf.MentalStatusExamination.PerceptionsAuditoryHallucinations,
                        PerceptionsVisualHallucinations = iaf.MentalStatusExamination.PerceptionsVisualHallucinations,
                        PerceptionsDelusions = iaf.MentalStatusExamination.PerceptionsDelusions,
                        PerceptionsParanoia = iaf.MentalStatusExamination.PerceptionsParanoia,
                        PerceptionsSuicidalAttempt = iaf.MentalStatusExamination.PerceptionsSuicidalAttempt,
                        PerceptionsSuicidalIdeations = iaf.MentalStatusExamination.PerceptionsSuicidalIdeations,
                        PerceptionsOthers = iaf.MentalStatusExamination.PerceptionsOthers,

                        MemoryShortTerm = iaf.MentalStatusExamination.MemoryShortTerm,
                        MemoryLongTerm = iaf.MentalStatusExamination.MemoryLongTerm,
                        MemoryOthers = iaf.MentalStatusExamination.MemoryOthers,

                        OrientationOrientedToTime = iaf.MentalStatusExamination.OrientationOrientedToTime,
                        OrientationOrientedToPerson = iaf.MentalStatusExamination.OrientationOrientedToPerson,
                        OrientationOrientedToPlace = iaf.MentalStatusExamination.OrientationOrientedToPlace,
                        OrientationDisorientedToTime = iaf.MentalStatusExamination.OrientationDisorientedToTime,
                        OrientationDisorientedToPerson = iaf.MentalStatusExamination.OrientationDisorientedToPerson,
                        OrientationDisorientedToPlace = iaf.MentalStatusExamination.OrientationDisorientedToPlace,
                        OrientationOthers = iaf.MentalStatusExamination.OrientationOthers,

                        JudgementGood = iaf.MentalStatusExamination.JudgementGood,
                        JudgementFair = iaf.MentalStatusExamination.JudgementFair,
                        JudgementPoor = iaf.MentalStatusExamination.JudgementPoor,

                        InsightGood = iaf.MentalStatusExamination.InsightGood,
                        InsightFair = iaf.MentalStatusExamination.InsightFair,
                        InsightPoor = iaf.MentalStatusExamination.InsightPoor
                    })
                    .FirstOrDefaultAsync() ?? new MentalStatusExaminationViewModel(),
            };

            // Return view with populated view model
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveHistoryOfPresentIllness(AssessmentFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
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
                patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                // patient.UpdatedAt = DateTime.Now;
                // patient.UpdatedBy = User.Identity.Name ?? "System";

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Initial assessment form has been saved successfully.";
                return View("EditInitialAssessmentForm", model);
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
        public async Task<IActionResult> SaveDrugHistory(AssessmentFormViewModel model)
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
                    // Remove existing entries
                    if (assessmentForm.DrugUses != null)
                    {
                        _context.DrugUses.RemoveRange(assessmentForm.DrugUses);
                    }

                    // Add new entries
                    foreach (var entry in model.DrugUseHistory.DrugUseEntries)
                    {
                        var drugUse = new DrugUse
                        {
                            InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                            SubstanceName = entry.SubstanceName,
                            Route = entry.Route,
                            QuantityPerDay = entry.QuantityPerDay,
                            Frequency = entry.Frequency,
                            FirstUse = entry.FirstUse,
                            EffectsWhenHigh = entry.EffectsWhenHigh,
                            EffectsWhenWanes = entry.EffectsWhenWanes,
                            CreatedAt = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        };

                        assessmentForm.DrugUses.Add(drugUse);
                    }
                }

                // Update patient status
                var patient = await _context.Patients.FindAsync(model.PatientId);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Drug history has been saved successfully.";

                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the drug history.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveMedicalHistory(AssessmentFormViewModel model)
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
                var patient = await _context.Patients.FindAsync(model.PatientId);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medical history has been saved successfully.";

                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the medical history.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePhysicalExam(AssessmentFormViewModel model)
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
                    .Include(iaf => iaf.PhysicalExam)
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

                // Update or create physical exam
                if (assessmentForm.PhysicalExam == null)
                {
                    assessmentForm.PhysicalExam = new PhysicalExam
                    {
                        InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                }
                else
                {
                    assessmentForm.PhysicalExam.UpdatedAt = DateTime.Now;
                    assessmentForm.PhysicalExam.UpdatedBy = User.Identity?.Name ?? "System";
                }

                // Update vital signs
                assessmentForm.PhysicalExam.BP = model.PhysicalExam.BP;
                assessmentForm.PhysicalExam.HR = model.PhysicalExam.HR;
                assessmentForm.PhysicalExam.RR = model.PhysicalExam.RR;
                assessmentForm.PhysicalExam.Temperature = model.PhysicalExam.Temperature;
                assessmentForm.PhysicalExam.O2 = model.PhysicalExam.O2;

                // Update system examination
                assessmentForm.PhysicalExam.SkinNormal = model.PhysicalExam.SkinNormal;
                assessmentForm.PhysicalExam.SkinFindings = model.PhysicalExam.SkinFindings;
                assessmentForm.PhysicalExam.ENTNormal = model.PhysicalExam.ENTNormal;
                assessmentForm.PhysicalExam.ENTFindings = model.PhysicalExam.ENTFindings;
                assessmentForm.PhysicalExam.ChestNormal = model.PhysicalExam.ChestNormal;
                assessmentForm.PhysicalExam.ChestFindings = model.PhysicalExam.ChestFindings;
                assessmentForm.PhysicalExam.LungsNormal = model.PhysicalExam.LungsNormal;
                assessmentForm.PhysicalExam.LungsFindings = model.PhysicalExam.LungsFindings;
                assessmentForm.PhysicalExam.CVSNormal = model.PhysicalExam.CVSNormal;
                assessmentForm.PhysicalExam.CVSFindings = model.PhysicalExam.CVSFindings;
                assessmentForm.PhysicalExam.AbdomenNormal = model.PhysicalExam.AbdomenNormal;
                assessmentForm.PhysicalExam.AbdomenFindings = model.PhysicalExam.AbdomenFindings;
                assessmentForm.PhysicalExam.GUTNormal = model.PhysicalExam.GUTNormal;
                assessmentForm.PhysicalExam.GUTFindings = model.PhysicalExam.GUTFindings;
                assessmentForm.PhysicalExam.ExtremitiesNormal = model.PhysicalExam.ExtremitiesNormal;
                assessmentForm.PhysicalExam.ExtremitiesFindings = model.PhysicalExam.ExtremitiesFindings;

                // Update patient status
                var patient = await _context.Patients.FindAsync(model.PatientId);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Physical examination has been saved successfully.";

                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the physical examination.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveDiagnosis(AssessmentFormViewModel model)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                    return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
                }

                // Get or create assessment form with related diagnosis data
                var assessmentForm = await _context.InitialAssessmentForms
                    .Include(iaf => iaf.Diagnosis)
                        .ThenInclude(d => d.SubstanceUseEntries)
                    .FirstOrDefaultAsync(iaf => iaf.PatientId == model.PatientId);

                // Create new assessment form if it doesn't exist
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

                // Update or create diagnosis
                if (assessmentForm.Diagnosis == null)
                {
                    assessmentForm.Diagnosis = new Diagnosis
                    {
                        InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                }
                else
                {
                    assessmentForm.Diagnosis.UpdatedAt = DateTime.Now;
                    assessmentForm.Diagnosis.UpdatedBy = User.Identity?.Name ?? "System";
                }

                // Clear existing substance use entries
                if (assessmentForm.Diagnosis.SubstanceUseEntries != null)
                {
                    _context.SubstanceUseEntries.RemoveRange(assessmentForm.Diagnosis.SubstanceUseEntries);
                }

                // Add new substance use entries
                if (model.Diagnosis?.SubstanceUses != null)
                {
                    foreach (var substance in model.Diagnosis.SubstanceUses)
                    {
                        if (!string.IsNullOrWhiteSpace(substance.SubstanceName) && !string.IsNullOrWhiteSpace(substance.Severity))
                        {
                            var entry = new SubstanceUseEntry
                            {
                                SubstanceName = substance.SubstanceName,
                                Severity = substance.Severity,
                                CreatedAt = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            };
                            assessmentForm.Diagnosis.SubstanceUseEntries.Add(entry);
                        }
                    }
                }

                // Update patient status to in progress
                var patient = await _context.Patients.FindAsync(model.PatientId);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                }

                // Save all changes
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Diagnosis has been saved successfully.";

                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (DbUpdateException dbEx)
            {
                // Log the database exception details here
                TempData["ErrorMessage"] = "A database error occurred while saving the diagnosis.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message: " + ex.Message);
                // Log the general exception details here
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the diagnosis.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveProblemList(AssessmentFormViewModel model)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                    return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
                }

                // Get or create assessment form with related problems
                var assessmentForm = await _context.InitialAssessmentForms
                    .Include(iaf => iaf.Problems)
                    .FirstOrDefaultAsync(iaf => iaf.PatientId == model.PatientId);

                // Create new assessment form if it doesn't exist
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

                // Clear existing problems
                if (assessmentForm.Problems != null)
                {
                    _context.ProblemLists.RemoveRange(assessmentForm.Problems);
                }

                // Add new problems from the viewmodel
                if (model.ProblemList?.Problems != null && model.ProblemList.Problems.Count > 0)
                {
                    foreach (var problemText in model.ProblemList.Problems)
                    {
                        if (!string.IsNullOrWhiteSpace(problemText))
                        {
                            var problem = new ProblemList
                            {
                                InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                                Problem = problemText,
                                CreatedAt = DateTime.Now,
                                CreatedBy = User.Identity?.Name ?? "System"
                            };

                            if (assessmentForm.Problems == null)
                            {
                                assessmentForm.Problems = new List<ProblemList>();
                            }

                            assessmentForm.Problems.Add(problem);
                        }
                    }
                }

                // Update patient status
                var patient = await _context.Patients.FindAsync(model.PatientId);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                }

                // Save all changes
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Problem list has been saved successfully.";

                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = "A database error occurred while saving the problem list.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the problem list.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveRecommendation(AssessmentFormViewModel model)
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
                    .Include(iaf => iaf.Recommendation)
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

                // Update or create recommendation
                if (assessmentForm.Recommendation == null)
                {
                    assessmentForm.Recommendation = new Recommendation
                    {
                        InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                }
                else
                {
                    assessmentForm.Recommendation.UpdatedAt = DateTime.Now;
                    assessmentForm.Recommendation.UpdatedBy = User.Identity?.Name ?? "System";
                }

                // Map ViewModel to Model
                assessmentForm.Recommendation.ProgramType = model.Recommendation.ProgramType;
                assessmentForm.Recommendation.ExpectedDuration = model.Recommendation.ExpectedDuration;
                assessmentForm.Recommendation.Reason = model.Recommendation.Reason;

                // Update patient status
                var patient = await _context.Patients.FindAsync(model.PatientId);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Recommendation has been saved successfully.";

                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = "A database error occurred while saving the recommendation.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the recommendation.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAssessment(int patientId)
        {
            try
            {
                // Find the patient by PatientId
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("Index");
                }

                // Get the assessment form with related data
                var assessmentForm = await _context.InitialAssessmentForms
                    .Include(iaf => iaf.Diagnosis)
                        .ThenInclude(d => d.SubstanceUseEntries)
                    .Include(iaf => iaf.Recommendation)
                    .FirstOrDefaultAsync(iaf => iaf.PatientId == patientId);

                if (assessmentForm == null)
                {
                    TempData["ErrorMessage"] = "Assessment form not found for this patient.";
                    return RedirectToAction("Index");
                }

                // Check if admission already exists for this patient
                var existingAdmission = await _context.Admissions.FirstOrDefaultAsync(a => a.PatientId == patientId && a.status != "Ended");

                if (existingAdmission == null)
                {
                    // Create new admission record
                    var admission = new Admission
                    {
                        PatientId = patientId,
                        AdmissionDate = DateTime.Now,

                        // Set IsDrugDependent based on substance use entries
                        IsDrugDependent = assessmentForm.Diagnosis?.SubstanceUseEntries?.Any() == true,

                        // Build diagnosis string from substance use entries
                        Diagnosis = assessmentForm.Diagnosis?.SubstanceUseEntries?.Any() == true
                            ? string.Join("; ", assessmentForm.Diagnosis.SubstanceUseEntries
                                .Select(sue => $"{sue.SubstanceName} - {sue.Severity}"))
                            : "No substance use diagnosis",

                        // Set recommendation from assessment
                        Recommendation = assessmentForm.Recommendation != null
                            ? $"{assessmentForm.Recommendation.ProgramType}"
                            : "No recommendation provided",

                        // Set status and audit fields
                        status = "Active",
                        CreatedBy = User.Identity.Name ?? "System",
                        CreatedAt = DateTime.Now
                    };

                    _context.Admissions.Add(admission);
                }
                else
                {
                    // Update existing admission with latest assessment data
                    existingAdmission.IsDrugDependent = assessmentForm.Diagnosis?.SubstanceUseEntries?.Any() == true;

                    existingAdmission.Diagnosis = assessmentForm.Diagnosis?.SubstanceUseEntries?.Any() == true
                        ? string.Join("; ", assessmentForm.Diagnosis.SubstanceUseEntries
                            .Select(sue => $"{sue.SubstanceName} - {sue.Severity}"))
                        : "No substance use diagnosis";

                    existingAdmission.Recommendation = assessmentForm.Recommendation != null
                        ? $"Program: {assessmentForm.Recommendation.ProgramType}, Duration: {assessmentForm.Recommendation.ExpectedDuration}, Reason: {assessmentForm.Recommendation.Reason}"
                        : "No recommendation provided";

                    existingAdmission.UpdatedBy = User.Identity.Name ?? "System";
                    existingAdmission.UpdatedAt = DateTime.Now;
                }

                // Update patient status to PendingApproval
                patient.PatientStatus = PatientStatusEnum.PendingApproval.ToString();
                assessmentForm.CompletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Assessment submitted for approval and admission record created/updated.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting the assessment.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveMentalStatusExamination(AssessmentFormViewModel model)
        {
            try
            {
                if (model?.PatientId == null)
                {
                    TempData["ErrorMessage"] = "Patient information missing. Cannot save mental status examination.";
                    return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId.Value });
                }

                // Get or create the InitialAssessmentForm for the patient
                var assessmentForm = await _context.InitialAssessmentForms
                    .Include(iaf => iaf.MentalStatusExamination)
                    .FirstOrDefaultAsync(iaf => iaf.PatientId == model.PatientId.Value);

                if (assessmentForm == null)
                {
                    assessmentForm = new InitialAssessmentForm
                    {
                        PatientId = model.PatientId.Value,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                    _context.InitialAssessmentForms.Add(assessmentForm);
                    await _context.SaveChangesAsync(); // get ID
                }

                // Ensure MentalStatusExamination navigation object exists
                if (assessmentForm.MentalStatusExamination == null)
                {
                    assessmentForm.MentalStatusExamination = new Models.MentalStatusExamination
                    {
                        InitialAssessmentFormId = assessmentForm.InitialAssessmentFormId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };
                    // If needed ensure it's tracked (adding to context is optional because parent is tracked)
                }
                else
                {
                    assessmentForm.MentalStatusExamination.UpdatedAt = DateTime.Now;
                    assessmentForm.MentalStatusExamination.UpdatedBy = User.Identity?.Name ?? "System";
                }

                // Map viewmodel -> model
                var vm = model.MentalStatusExamination ?? new MentalStatusExaminationViewModel();
                var ms = assessmentForm.MentalStatusExamination;

                ms.GeneralAppearanceNeat = vm.GeneralAppearanceNeat;
                ms.GeneralAppearanceDishevelled = vm.GeneralAppearanceDishevelled;
                ms.GeneralAppearanceInappropriate = vm.GeneralAppearanceInappropriate;
                ms.GeneralAppearanceOthers = vm.GeneralAppearanceOthers;

                ms.SpeechNormal = vm.SpeechNormal;
                ms.SpeechRapid = vm.SpeechRapid;
                ms.SpeechSlow = vm.SpeechSlow;
                ms.SpeechIncoherent = vm.SpeechIncoherent;
                ms.SpeechOthers = vm.SpeechOthers;

                ms.BehaviorRelaxed = vm.BehaviorRelaxed;
                ms.BehaviorCooperative = vm.BehaviorCooperative;
                ms.BehaviorSuspicious = vm.BehaviorSuspicious;
                ms.BehaviorPreoccupied = vm.BehaviorPreoccupied;
                ms.BehaviorOthers = vm.BehaviorOthers;

                ms.ViolenceRelaxed = vm.ViolenceRelaxed;
                ms.ViolenceRestless = vm.ViolenceRestless;
                ms.ViolenceClenchedFist = vm.ViolenceClenchedFist;
                ms.ViolenceRaisedVoice = vm.ViolenceRaisedVoice;
                ms.ViolenceOthers = vm.ViolenceOthers;

                ms.MoodSad = vm.MoodSad;
                ms.MoodAnxious = vm.MoodAnxious;
                ms.MoodHappy = vm.MoodHappy;
                ms.MoodFearful = vm.MoodFearful;
                ms.MoodHelpless = vm.MoodHelpless;
                ms.MoodHopeless = vm.MoodHopeless;
                ms.MoodAngry = vm.MoodAngry;
                ms.MoodOthers = vm.MoodOthers;

                ms.AffectAppropriate = vm.AffectAppropriate;
                ms.AffectInappropriate = vm.AffectInappropriate;
                ms.AffectFlat = vm.AffectFlat;
                ms.AffectBlunted = vm.AffectBlunted;
                ms.AffectOthers = vm.AffectOthers;

                ms.ThoughtsNormal = vm.ThoughtsNormal;
                ms.ThoughtsFlightOfIdeas = vm.ThoughtsFlightOfIdeas;
                ms.ThoughtsPreoccupied = vm.ThoughtsPreoccupied;
                ms.ThoughtsOthers = vm.ThoughtsOthers;

                ms.CognitionConscious = vm.CognitionConscious;
                ms.CognitionConfused = vm.CognitionConfused;
                ms.CognitionDrowsy = vm.CognitionDrowsy;
                ms.CognitionOthers = vm.CognitionOthers;

                ms.PerceptionsIllusions = vm.PerceptionsIllusions;
                ms.PerceptionsAuditoryHallucinations = vm.PerceptionsAuditoryHallucinations;
                ms.PerceptionsVisualHallucinations = vm.PerceptionsVisualHallucinations;
                ms.PerceptionsDelusions = vm.PerceptionsDelusions;
                ms.PerceptionsParanoia = vm.PerceptionsParanoia;
                ms.PerceptionsSuicidalAttempt = vm.PerceptionsSuicidalAttempt;
                ms.PerceptionsSuicidalIdeations = vm.PerceptionsSuicidalIdeations;
                ms.PerceptionsOthers = vm.PerceptionsOthers;

                ms.MemoryShortTerm = vm.MemoryShortTerm;
                ms.MemoryLongTerm = vm.MemoryLongTerm;
                ms.MemoryOthers = vm.MemoryOthers;

                ms.OrientationOrientedToTime = vm.OrientationOrientedToTime;
                ms.OrientationOrientedToPerson = vm.OrientationOrientedToPerson;
                ms.OrientationOrientedToPlace = vm.OrientationOrientedToPlace;
                ms.OrientationDisorientedToTime = vm.OrientationDisorientedToTime;
                ms.OrientationDisorientedToPerson = vm.OrientationDisorientedToPerson;
                ms.OrientationDisorientedToPlace = vm.OrientationDisorientedToPlace;
                ms.OrientationOthers = vm.OrientationOthers;

                ms.JudgementGood = vm.JudgementGood;
                ms.JudgementFair = vm.JudgementFair;
                ms.JudgementPoor = vm.JudgementPoor;

                ms.InsightGood = vm.InsightGood;
                ms.InsightFair = vm.InsightFair;
                ms.InsightPoor = vm.InsightPoor;

                // Update parent assessment audit fields
                assessmentForm.UpdatedAt = DateTime.Now;
                assessmentForm.UpdatedBy = User.Identity?.Name ?? "System";

                // Update patient status to InProgress
                var patient = await _context.Patients.FindAsync(model.PatientId.Value);
                if (patient != null)
                {
                    patient.PatientStatus = PatientStatusEnum.InProgress.ToString();
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mental status examination saved successfully.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model.PatientId.Value });
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Database error occurred while saving mental status examination.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model?.PatientId });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while saving the mental status examination.";
                return RedirectToAction("EditInitialAssessmentForm", new { id = model?.PatientId });
            }
        }
    }
}