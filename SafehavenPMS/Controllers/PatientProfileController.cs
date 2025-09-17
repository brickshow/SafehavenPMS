using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.ViewModel.PatientProfile;
using System.Linq;

namespace SafehavenPMS.Controllers
{
    public class PatientProfileController : Controller
    {
        private readonly SafehavenPMSContext _context;

        //Constructor
        public PatientProfileController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Patient ID not found.";
                // Return an empty model to avoid null reference
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
                // Return an empty model to avoid null reference
                return View(new PatientProfilePageViewModel());
            }

            // Fetch InitialAssessmentForm and ProblemList for TreatmentPlan
            var assessment = await _context.PsychiatricAssessments
                .Include(a => a.ProblemLists)
                    .ThenInclude(p => p.Goals)
                .FirstOrDefaultAsync(a => a.PatientId == patient.PatientId);

            var treatmentPlanViewModel = new PatientTreatmentPlanTabViewModel();

            // Fetch all interventions for the patient
            var interventions = await _context.Interventions
                .Include(i => i.ServiceType)
                .Include(i => i.ServiceModality)
                .Where(i => i.PatientId == patient.PatientId)
                .ToListAsync();

            var interventionsByProblem = interventions
                .GroupBy(i => i.PsyProblemListId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(i => new InterventionViewModel
                    {
                        InterventionId = i.InterventionId,
                        ServiceTypeName = i.ServiceType?.ServiceName,
                        ServiceModalityName = i.ServiceModality?.ServiceName,
                        Description = i.Description,
                        Frequency = i.DurationFrequency,
                        Status = i.Status,
                        NotedBy = i.NotedBy,
                        DateAdded = i.DateAdded
                    }).ToList()
                );

            if (assessment?.ProblemLists != null)
            {
                foreach (var problem in assessment.ProblemLists)
                {
                    var problemVm = new ProblemViewModel
                    {
                        PsyProblemListId = problem.PsyProblemListId,
                        InitialAssessmentFormId = assessment.PsychiatricAssessmentId,
                        Problems = problem.Problem,
                        Status = problem.Status,
                        Goals = problem.Goals?.Select(g => new GoalViewModel
                        {
                            GoalId = g.GoalId,
                            Description = g.Description,
                            Status = g.Status,
                            NotedBy = g.NotedBy,
                            TargetDate = g.TargetDate
                        }).ToList() ?? new List<GoalViewModel>(),
                        Interventions = interventionsByProblem.ContainsKey(problem.PsyProblemListId)
                            ? interventionsByProblem[problem.PsyProblemListId]
                            : new List<InterventionViewModel>()
                    };
                    treatmentPlanViewModel.Problems.Add(problemVm);
                }
            }

            // <<-- NEW: include medication orders as InterventionViewModel entries per problem
            var meds = await _context.MedicationOrders
                        .Include(m => m.Medicine)
                        .Where(m => m.PatientId == patient.PatientId)
                        .ToListAsync();

            var medsByProblem = meds
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
                            ? $"{m.Medicine.GenericName} ({m.Medicine.BrandName}) - {m.Medicine.Form} {m.Medicine.Strength} {m.Medicine.Unit}"
                            : null,
                        UnitPerDose = m.UnitPerDose + (m.Medicine != null ? $" {m.Medicine.Form}" : string.Empty)
                    }).ToList()
                );

            // attach medication entries to matching problems in the treatment plan viewmodel
            foreach (var problemVm in treatmentPlanViewModel.Problems)
            {
                var key = problemVm.PsyProblemListId;
                if (medsByProblem.ContainsKey(key))
                {
                    // append medication-derived intervention viewmodels
                    problemVm.Interventions.AddRange(medsByProblem[key]);
                }

                // Sort combined interventions by DateAdded/CreatedAt (most recent first)
                problemVm.Interventions = problemVm.Interventions
                    .OrderByDescending(i => i.DateAdded ?? DateTime.MinValue)
                    .ToList();
            }
            // -- END NEW

            var viewModel = new PatientProfilePageViewModel
            {
                PatientId = patient.PatientId,
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
                    PatientId = patient.PatientId,
                    FirstName = patient.Firstname,
                    LastName = patient.Lastname,
                    MiddleName = patient.MiddleName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = CalculateAge(patient.DateOfBirth),
                    MaritalStatus = patient.MaritalStatus,
                    Occupation = patient.Occupation,
                    Religion = patient.Religion,
                    Sex = patient.Sex,
                    PhoneNumber = patient.PhoneNumber,
                    Address = patient.Address,
                    FamilyConstellation = patient.IntakeForm?.FamilyMembers?
                        .Select(fm => new FamilyConstellationViewModel
                        {
                            Name = fm.Name,
                            Relationship = fm.Relationship,
                            Age = fm.Age.ToString(),
                            Comments = fm.Comments,
                        }).ToList() ?? new List<FamilyConstellationViewModel>()
                },
                MedicalHistoryTab = new PatientMedicalHistoryTabViewModel(),
                ClinicalFormTab = new PatientClinicalFormTabViewModel(),

                TreatmentPlanTab = new PatientTreatmentPlanTabViewModel
                {
                    Problems = treatmentPlanViewModel.Problems
                },

                ProgressNotesTab = new PatientProgressNotesTabViewModel(),
                ActivityLogTab = new PatientActivityLogTabViewModel(),
                Interventions = interventions
            };


            //Display the Intervention in console
            foreach (var intervention in interventions)
            {
                Console.WriteLine($"Intervention ID: {intervention.InterventionId}, Description: {intervention.Description}");
            }


            //ViewBag for patient ID
            ViewBag.PatientId = patient.PatientId;

            // Strongly-typed select lists for the view (avoid anonymous/SelectList runtime issues)
            var serviceTypesList = await _context.ServiceTypes
                                        .Where(st => st.Status == "Active")
                                        .ToListAsync();

            ViewBag.ServiceTypes = serviceTypesList
                .Select(st => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = st.ServiceTypeId.ToString(),
                    Text = st.ServiceName
                })
                .ToList();

            var servicesList = await _context.Services
                                    .ToListAsync();

            ViewBag.Services = servicesList
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
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
    }
}
