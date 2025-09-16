using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.ViewModel.PatientProfile;

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
                        Interventions = new List<InterventionViewModel>() // TODO: Map interventions if you have an interventions table
                    };
                    treatmentPlanViewModel.Problems.Add(problemVm);
                }
            }

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
                ActivityLogTab = new PatientActivityLogTabViewModel()
            };

            //ViewBag for patient ID
            ViewBag.PatientId = patient.PatientId;
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
