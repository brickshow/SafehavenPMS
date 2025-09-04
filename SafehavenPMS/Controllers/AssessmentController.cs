using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using SafehavenPMS.ViewModel.Assessment;


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
            var pendingAssessment = patientsData
                .Where(p => p.PatientStatus == PatientStatusEnum.PendingAssessment.ToString() ||
                            p.PatientStatus == PatientStatusEnum.InProgress.ToString())
                .Select(p =>
                {
                    var appointment = p.NewAppointments.FirstOrDefault();
                    var physician = p.ClinicalStaffPatients.FirstOrDefault(csp => csp.ClinicalStaff.Position == "Physician")?.ClinicalStaff;

                    return new PendingAssessmentViewModel
                    {
                        PatientId = p.PatientId,
                        PhysicianId = appointment?.ClinicalStaffID ?? 0,
                        PhysicianName = physician != null ? $"{physician.Firstname} {physician.Lastname}" : "-",
                        Type = appointment?.Type ?? "-",
                        PatientName = $"{p.Firstname} {p.Lastname}",
                        Date = appointment?.ScheduleDate,
                        Time = appointment?.ScheduleTime,
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
            var patient = await _context.Patients
            .Include(p => p.IntakeForm)
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
    }
}