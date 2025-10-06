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
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace SafehavenPMS.Controllers
{
[Authorize]
    public class PatientProfileController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly CloudinaryServices _cloudSvc;

        //Constructor
        public PatientProfileController(SafehavenPMSContext context, CloudinaryServices cloudSvc)
        {
            _context = context;
            _cloudSvc = cloudSvc;
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

            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists) return NotFound("Patient not found.");

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
    }
}



