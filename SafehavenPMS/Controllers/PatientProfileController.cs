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
                return View();
            }

            // Fetch patient details from the database
            var patient = await _context.Patients
                                .Include(c => c.ClinicalStaffPatients)
                                    .ThenInclude(s => s.ClinicalStaff)
                                .Include(c => c.IntakeForm)
                                    .ThenInclude(i => i.FamilyMembers)
                                .FirstOrDefaultAsync(i => i.PatientId == id);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return View();
            }


            var viewModel = new ViewModel.PatientProfilePageViewModel
            {
                PatientId = patient.PatientId,
                PatientName = $"{patient.Firstname} {patient.Lastname}",
                OverViewTab = new PatientOverViewTabViewModel
                {
                    // Populate with actual data as needed
                    FoodAllergies = new List<string> { "Peanuts", "Shellfish" },
                    DrugAllergies = new List<string> { "Penicillin" },
                    ActiveMedications = new List<string> { "Aspirin", "Lisinopril" },
                    // TreatmentTeams expects a list of TreatmentTeamMemberViewModel instances.
                    // Provide an empty list for now or create TreatmentTeamMemberViewModel objects as needed.
                    TreatmentTeams = new List<TreatmentTeamMemberViewModel>()
                },

                PersonalInfoTab = new PatientPersonalInfoTabViewModel()
                {
                    // Populate with actual data
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

                    // Populate family members from FamilyMember entity
                    FamilyConstellation = patient.IntakeForm?.FamilyMembers?
                        .Select(fm => new FamilyConstellationViewModel
                        {
                            Name = fm.Name,
                            Relationship = fm.Relationship,
                            Age = fm.Age.ToString(),
                            Comments = fm.Comments,
                        }).ToList() ?? new List<FamilyConstellationViewModel>()
                },
                MedicalHistoryTab = new ViewModel.PatientMedicalHistoryTabViewModel(),
                ClinicalFormTab = new ViewModel.PatientClinicalFormTabViewModel(),
                TreatmentPlanTab = new ViewModel.PatientTreatmentPlanTabViewModel(),
                ProgressNotesTab = new ViewModel.PatientProgressNotesTabViewModel(),
                ActivityLogTab = new ViewModel.PatientActivityLogTabViewModel()
            };



            // Return the entity directly to the view (avoid mapping to properties that may not exist)
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
