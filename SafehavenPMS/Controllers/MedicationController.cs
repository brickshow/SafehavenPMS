using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Enum;
using SafehavenPMS.Helpers; // add this
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Controllers
{
[Authorize]
    public class MedicationController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private const string TempOrdersSessionKey = "TempMedicationOrders"; // add this

        public MedicationController(SafehavenPMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(
            string medSearch = null, string medStatus = null, string medSortBy = null, string medSortOrder = null,
            string orderSearch = null, string orderStatus = null, string orderSortBy = null, string orderSortOrder = null,
            string logSearch = null, string logStatus = null, string logSortBy = null, string logSortOrder = null)
        {
            // --- Medicines ---
            var medicines = await _context.Medicines.ToListAsync();
            if (!string.IsNullOrEmpty(medSearch))
                medicines = medicines.Where(m => (m.GenericName ?? "").ToLower().Contains(medSearch.ToLower()) ||
                                                 (m.BrandName ?? "").ToLower().Contains(medSearch.ToLower())).ToList();
            if (!string.IsNullOrEmpty(medStatus))
                medicines = medicines.Where(m => m.Status == medStatus).ToList();
            if (!string.IsNullOrEmpty(medSortBy))
            {
                if (medSortBy == "Name")
                    medicines = medSortOrder == "ascending" ? medicines.OrderBy(m => m.GenericName).ToList() : medicines.OrderByDescending(m => m.GenericName).ToList();
                else if (medSortBy == "DateAdded")
                    medicines = medSortOrder == "ascending" ? medicines.OrderBy(m => m.DateAdded).ToList() : medicines.OrderByDescending(m => m.DateAdded).ToList();
            }

            // --- Medication Orders ---
            var medicationOrders = await _context.MedicationOrders
                .Include(m => m.Patient)
                .Include(m => m.Medicine)
                .ToListAsync();
            if (!string.IsNullOrEmpty(orderSearch))
                medicationOrders = medicationOrders.Where(o => (o.Patient?.Firstname + " " + o.Patient?.Lastname).ToLower().Contains(orderSearch.ToLower())).ToList();
            if (!string.IsNullOrEmpty(orderStatus))
                medicationOrders = medicationOrders.Where(o => o.Status == orderStatus).ToList();
            if (!string.IsNullOrEmpty(orderSortBy))
            {
                if (orderSortBy == "Name")
                    medicationOrders = orderSortOrder == "ascending" ? medicationOrders.OrderBy(o => o.Patient.Firstname).ToList() : medicationOrders.OrderByDescending(o => o.Patient.Firstname).ToList();
                // Add more sort options as needed
            }

            // --- Administration Logs (today) ---
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            // Pull today's administration (taken) statuses
            var administrationLogs = await _context.AdministrationLogs
                .Where(a => a.AdministrationDate >= todayStart && a.AdministrationDate < todayEnd)
                .ToListAsync();

            // Fast lookup by MedicationOrderId
            var adminLogDict = administrationLogs
                .GroupBy(a => a.MedicationOrderId)
                .ToDictionary(g => g.Key, g => g.First());

            // Map to view model
            var model = new MedicationPageViewModel
            {
                Medicines = medicines.Select(m => new MedicineViewModel
                {
                    MedicineId = m.MedicineId,
                    GenericName = m.GenericName,
                    BrandName = m.BrandName,
                    Form = m.Form,
                    Strength = m.Strength,
                    Unit = m.Unit,
                    Price = m.Price,
                    Status = m.Status,
                    DateAdded = m.DateAdded
                }).ToList(),
                MedicationOrders = medicationOrders.Select(m => new MedicationOrderViewModel
                {
                    MedicationOrderId = m.MedicationOrderId,
                    PatientId = m.PatientId,
                    PatientName = m.Patient != null ? m.Patient.Firstname + " " + m.Patient.Lastname : string.Empty,
                    MedicineId = m.MedicineId,
                    MedicineName = m.Medicine != null ? $"{m.Medicine.GenericName} ({m.Medicine.BrandName}) - {m.Medicine.Strength} {m.Medicine.Unit} {m.Medicine.Form}" : string.Empty,
                    UnitPerDoseDisplay = $"{m.UnitPerDose} {m.Medicine?.Form}",
                    Note = m.Note,
                    ScheduledType = m.ScheduledType,
                    ScheduleTimes = string.Join(", ", new[] { m.Breakfast ? "BF" : null, m.Lunch ? "L" : null, m.Dinner ? "D" : null, m.Bedtime ? "BT" : null }.Where(x => x != null)),
                    DaysInterval = m.DaysInterval,
                    Breakfast = m.Breakfast,
                    Lunch = m.Lunch,
                    Dinner = m.Dinner,
                    Bedtime = m.Bedtime,
                    Status = m.Status,
                    CreatedBy = m.CreatedBy,
                    StartDate = m.StartDate,
                    DiscontinueDate = m.DiscontinueDate,
                    NoDiscontinueDate = m.NoDiscontinueDate
                }).ToList(),
                AdministrationLogs = medicationOrders
                    .GroupBy(a => a.PatientId)
                    .Select(g => new AdministrationLogViewModel
                    {
                        PatientId = g.Key,
                        PatientName = g.First().Patient != null ? $"{g.First().Patient.Firstname} {g.First().Patient.Lastname}" : string.Empty,
                        TotalMeds = g.Count(),
                        ScheduleTimes = string.Join(", ", new[]
                        {
                            g.Any(x => x.Breakfast) ? "BF" : null,
                            g.Any(x => x.Lunch) ? "L" : null,
                            g.Any(x => x.Dinner) ? "D" : null,
                            g.Any(x => x.Bedtime) ? "BT" : null
                        }.Where(x => x != null)),
                        Medications = g.Select(m =>
                        {
                            adminLogDict.TryGetValue(m.MedicationOrderId, out var taken);
                            return new MedicationOrderViewModel
                            {
                                MedicationOrderId = m.MedicationOrderId,
                                PatientId = m.PatientId,
                                MedicineId = m.MedicineId,
                                MedicineName = m.Medicine != null ? $"{m.Medicine.GenericName} ({m.Medicine.BrandName}) - {m.Medicine.Strength} {m.Medicine.Unit} {m.Medicine.Form}" : string.Empty,
                                UnitPerDoseDisplay = $"{m.UnitPerDose} {m.Medicine?.Form}",
                                Note = m.Note,
                                ScheduledType = m.ScheduledType,
                                Breakfast = m.Breakfast,
                                Lunch = m.Lunch,
                                Dinner = m.Dinner,
                                Bedtime = m.Bedtime,
                                StartDate = m.StartDate,
                                DiscontinueDate = m.DiscontinueDate,
                                NoDiscontinueDate = m.NoDiscontinueDate,
                                Status = m.Status,

                                // Populate taken flags from today's log (defaults false if none yet)
                                BreakfastTaken = taken?.BreakfastTaken ?? false,
                                LunchTaken = taken?.LunchTaken ?? false,
                                DinnerTaken = taken?.DinnerTaken ?? false,
                                BedtimeTaken = taken?.BedtimeTaken ?? false
                            };
                        }).ToList()
                    }).ToList()
            };

            // Pass filter/sort/search state to view
            ViewBag.MedSearch = medSearch;
            ViewBag.MedStatus = medStatus;
            ViewBag.MedSortBy = medSortBy;
            ViewBag.MedSortOrder = medSortOrder;
            ViewBag.OrderSearch = orderSearch;
            ViewBag.OrderStatus = orderStatus;
            ViewBag.OrderSortBy = orderSortBy;
            ViewBag.OrderSortOrder = orderSortOrder;
            ViewBag.LogSearch = logSearch;
            ViewBag.LogStatus = logStatus;
            ViewBag.LogSortBy = logSortBy;
            ViewBag.LogSortOrder = logSortOrder;

            return View(model);
        }


        //View for Add medicine
        public IActionResult AddMedicine()
        {
            return View();
        }

        //Action on adding new medicine
        [HttpPost]
        public async Task<IActionResult> AddMedicine(MedicineViewModel model)
        {
            //Check if model state is valie
            if (!ModelState.IsValid)
            {
                // Log ModelState errors to console
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;

                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                    }
                }
                return View(model); // Pass model back to preserve entered data
            }

            //Saving view model to Model
            var med = new Medicine
            {
                MedicineId = model.MedicineId,
                GenericName = model.GenericName,
                BrandName = model.BrandName,
                Form = model.Form,
                Strength = model.Strength,
                Unit = model.Unit,
                Price = model.Price,
                DateAdded = DateTime.Now
            };

            //Handle error when saving to database
            try
            {
                await _context.Medicines.AddAsync(med);
                await _context.SaveChangesAsync();//Save changes to database
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("Index");
        }

        //Method to edi medicine
        [HttpGet]
        public async Task<IActionResult> EditMedicine(int id)
        {
            var med = await _context.Medicines.FirstOrDefaultAsync(s => s.MedicineId == id);

            if (med == null)
            {
                TempData["Error"] = "No Medicine Found";
                return View("Index");
            }

            var vm = new MedicineViewModel
            {
                MedicineId = med.MedicineId,
                GenericName = med.GenericName,
                BrandName = med.BrandName,
                Form = med.Form,
                Strength = med.Strength,
                Unit = med.Unit,
                Price = med.Price,
                Status = med.Status,
            };

            return View(vm);
        }

        public async Task<IActionResult> EditMedicine(int id, MedicineViewModel model)
        {
            var med = await _context.Medicines.FirstOrDefaultAsync(a => a.MedicineId == id);
            if (med == null)
            {
                TempData["Error"] = "Medicine not found!";
                return RedirectToAction("Index");
            }

            // Update fields
            med.GenericName = model.GenericName;
            med.BrandName = model.BrandName;
            med.Form = model.Form;
            med.Strength = model.Strength;
            med.Unit = model.Unit;
            med.Price = model.Price;
            med.Status = model.Status;

            try
            {
                await _context.SaveChangesAsync(); // EF tracks the changes
                TempData["SuccessMessage"] = "Medicine updated successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                TempData["Error"] = "Failed to update medicine.";
            }

            return RedirectToAction("Index");
        }

        //Action to delete/deactivate Medicine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateMedicine(int id)
        {
            //Find the medicine from database
            var med = await _context.Medicines.FindAsync(id);

            if (med == null)
            {
                TempData["Error"] = "Medicine not found";
                return View();
            }

            //update the database 
            med.Status = Enum.MedicineStatus.Inactive.ToString();

            try
            {
                //Insert to DB
                _context.Update(med);

                //Save Changes
                await _context.SaveChangesAsync();
                TempData["Error"] = "Medicine succesfully Deactivated";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                TempData["Error"] = "Error Deactivating Medicine! Please try again";
            }

            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> AddMedicationOrder(int? medicineId, int? patientId, int? problemId)
        {
            var medicines = await _context.Medicines.ToListAsync();
            var patients = await _context.Patients
                                 .Where(s => s.PatientStatus == Enum.PatientStatusEnum.Admitted.ToString() ||
                                             s.PatientStatus == Enum.PatientStatusEnum.InTreatment.ToString())
                                 .ToListAsync();

            // Build SelectList for ViewBag
            ViewBag.PatientList = new SelectList(
                patients.Select(p => new
                {
                    PatientId = p.PatientId,
                    FullName = (p.Firstname ?? "") + " " + (p.Lastname ?? "")
                }),
                "PatientId",
                "FullName",
                patientId
            );

            // Build SelectList for medicines in the format: Generic Name (Brand Name) - Form Strength Unit
            ViewBag.MedicineList = new SelectList(
                medicines.Where(a => a.Status == Enum.MedicineStatus.Active.ToString())
                .Select(m => new
                {
                    MedicineId = m.MedicineId,
                    DisplayName = $"{m.GenericName} ({m.BrandName}) - {m.Form} {m.Strength.ToString("0.#")} {m.Unit}"
                }),
                "MedicineId",
                "DisplayName",
                medicineId
            );

            // Read temporary orders from session
            var tempList = HttpContext.Session.GetObject<List<MedicationOrderViewModel>>(TempOrdersSessionKey) ?? new List<MedicationOrderViewModel>();

            // Fill display names using loaded patients/medicines so view shows readable items
            var patientDict = patients.ToDictionary(p => p.PatientId, p => $"{p.Firstname} {p.Lastname}");
            var medicineDict = medicines.ToDictionary(m => m.MedicineId, m => $"{m.GenericName} ({m.BrandName}) - {m.Form} {m.Strength} {m.Unit}");

            foreach (var t in tempList)
            {
                if (string.IsNullOrWhiteSpace(t.PatientName) && patientDict.ContainsKey(t.PatientId))
                    t.PatientName = patientDict[t.PatientId];

                if (string.IsNullOrWhiteSpace(t.MedicineName) && medicineDict.ContainsKey(t.MedicineId))
                    t.MedicineName = medicineDict[t.MedicineId];
            }

            var patientExists = patients.FirstOrDefault(p => p.PatientId == patientId);
            if (patientId.HasValue && patientExists == null)
            {
                ViewBag.PatientName = "Unknown Patient";
            }
            else
            {
                ViewBag.PatientName = patientExists.Firstname  + " " + patientExists.Lastname;
            }

            ViewBag.SelectedPatientID = patientId;
            ViewBag.TempOrders = tempList;
            ViewBag.ProblemId = problemId; // Pass problemId to ViewBag if needed in the view

            // Retain problemId in the view model
            return View(new MedicationOrderViewModel { PatientId = patientId ?? 0, PsyProblemListId = problemId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicationOrder(MedicationOrderViewModel model)
        {
            try
            {
                // ?? Custom Validation: DiscontinueDate must be greater than StartDate
                if (!model.NoDiscontinueDate && model.DiscontinueDate.HasValue)
                {
                    if (model.DiscontinueDate.Value.Date <= model.StartDate.Date)
                    {
                        ModelState.AddModelError(nameof(model.DiscontinueDate), "Discontinue Date must be later than Start Date.");
                    }
                }

                // 1?? If model is invalid ? go back to Index instead of staying here
                if (!ModelState.IsValid)
                {
                    ViewBag.PatientList = new SelectList(
                                          _context.Patients.Select(p => new
                                          {
                                              PatientId = p.PatientId,
                                              FullName = (p.Firstname ?? "") + " " + (p.Lastname ?? "")
                                          }),
                                          "PatientId",
                                          "FullName",
                                          model.PatientId // <-- retain selected patient
                                      );

                    // Build SelectList for medicines in the format: Generic Name (Brand Name) - Form Strength Unit
                    ViewBag.MedicineList = new SelectList(
                                            _context.Medicines
                                            .Where(a => a.Status == Enum.MedicineStatus.Active.ToString())
                                            .Select(m => new
                                            {
                                                MedicineId = m.MedicineId,
                                                DisplayName = $"{m.GenericName} ({m.BrandName}) - {m.Form} {m.Strength.ToString("0.#")} {m.Unit}"
                                            }),
                                            "MedicineId",
                                            "DisplayName",
                                            model.MedicineId
                                        );


                    TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
                    return View(model);
                }

                // ?? Determine Status using simple if/else
                string status;
                if (model.StartDate.Date == DateTime.Today)
                {
                    status = MedicationOrderStatus.Active.ToString();
                }
                else if (model.StartDate.Date > DateTime.Today)
                {
                    status = MedicationOrderStatus.NotStarted.ToString();
                }
                else
                {
                    status = MedicationOrderStatus.Active.ToString();
                }

                // 2?? Map to entity
                var medicationOrder = new MedicationOrder
                {
                    PatientId = model.PatientId,
                    PsyProblemListId = model.PsyProblemListId, // map problem list if applicable
                    MedicineId = model.MedicineId,
                    UnitPerDose = model.UnitPerDose,
                    Note = model.Note,
                    ScheduledType = model.ScheduledType,
                    DaysInterval = model.ScheduledType == "NonDaily" ? model.DaysInterval : null,
                    Breakfast = model.Breakfast,
                    Lunch = model.Lunch,
                    Dinner = model.Dinner,
                    Bedtime = model.Bedtime,
                    StartDate = model.StartDate,
                    DiscontinueDate = model.NoDiscontinueDate ? null : model.DiscontinueDate,
                    NoDiscontinueDate = model.NoDiscontinueDate,
                    CreatedAt = DateTime.Now,
                    Status = status,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                // Read existing temp list from session, append, save back
                var tempList = HttpContext.Session.GetObject<List<MedicationOrder>>(TempOrdersSessionKey) ?? new List<MedicationOrder>();
                tempList.Add(medicationOrder);
                HttpContext.Session.SetObject(TempOrdersSessionKey, tempList);

                TempData["SuccessMessage"] = "Medication order added to temporary list. Click Submit Order to save to database.";
                Console.WriteLine($"Patient ID: {model.PatientId}");
                return RedirectToAction("AddMedicationOrder", new { patientId = model.PatientId, problemId = model.PsyProblemListId });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction("AddMedicationOrder", new { patientId = model.PatientId, problemId = model.PsyProblemListId });
            }
        }

        //Action to mark Medication order as Completed
        public IActionResult Completed(int id)
        {
            var medOrder = _context.MedicationOrders.FirstOrDefault(m => m.MedicationOrderId == id);

            if (medOrder == null)
            {
                TempData["Error"] = "Medication Order not found";
                return RedirectToAction("Index");
            }

            medOrder.Status = MedicationOrderStatus.Discontinued.ToString();

            try
            {
                _context.Update(medOrder);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Medication order marked as Completed.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                TempData["Error"] = "Failed to update medication order status.";
            }

            return RedirectToAction("Index");
        }

        // GET: Medication/EditMedicationOrder/5
        public async Task<IActionResult> EditMedicationOrder(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid Medication Order ID";
                return RedirectToAction("Index");
            }

            var order = await _context.MedicationOrders
                .Include(m => m.Medicine)
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.MedicationOrderId == id);

            if (order == null)
            {
                TempData["Error"] = "Medication Order not found";
                return RedirectToAction("Index");
            }

            // Map entity to ViewModel
            var viewModel = new MedicationOrderViewModel
            {
                MedicationOrderId = order.MedicationOrderId,
                PatientId = order.PatientId,
                MedicineId = order.MedicineId,
                UnitPerDose = order.UnitPerDose,
                Note = order.Note,
                ScheduledType = order.ScheduledType,
                DaysInterval = order.DaysInterval,
                Breakfast = order.Breakfast,
                Lunch = order.Lunch,
                Dinner = order.Dinner,
                Bedtime = order.Bedtime,
                StartDate = order.StartDate,
                DiscontinueDate = order.DiscontinueDate,
                NoDiscontinueDate = order.NoDiscontinueDate
            };

            // Repopulate dropdowns
            ViewBag.PatientList = new SelectList(
                _context.Patients.Select(p => new
                {
                    PatientId = p.PatientId,
                    FullName = (p.Firstname ?? "") + " " + (p.Lastname ?? "")
                }),
                "PatientId",
                "FullName",
                order.PatientId
            );

            ViewBag.MedicineList = new SelectList(
                _context.Medicines
                .Where(a => a.Status == Enum.MedicineStatus.Active.ToString()).Select(m => new
                {
                    MedicineId = m.MedicineId,
                    DisplayName = $"{m.GenericName} ({m.BrandName}) - {m.Form} {m.Strength.ToString("0.#")} {m.Unit}"
                }),
                "MedicineId",
                "DisplayName",
                order.MedicineId
            );

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicationOrder(int id, MedicationOrderViewModel model)
        {
            try
            {
                // ?? Custom Validation: DiscontinueDate must be later than StartDate
                if (!model.NoDiscontinueDate && model.DiscontinueDate.HasValue)
                {
                    if (model.DiscontinueDate.Value.Date <= model.StartDate.Date)
                    {
                        ModelState.AddModelError(
                            nameof(model.DiscontinueDate),
                            "Discontinue Date must be later than Start Date."
                        );
                    }
                }

                // ?? Check for general model validation errors
                if (!ModelState.IsValid)
                {     // Log ModelState errors to console
                    foreach (var entry in ModelState)
                    {
                        var key = entry.Key;
                        var errors = entry.Value.Errors;

                        foreach (var error in errors)
                        {
                            Console.WriteLine($"Field: {key} - Error: {error.ErrorMessage}");
                        }
                    }

                    // Repopulate dropdowns so the view can render correctly
                    ViewBag.PatientList = new SelectList(
                        _context.Patients.Select(p => new
                        {
                            PatientId = p.PatientId,
                            FullName = (p.Firstname ?? "") + " " + (p.Lastname ?? "")
                        }),
                        "PatientId",
                        "FullName",
                        model.PatientId
                    );

                    ViewBag.MedicineList = new SelectList(
                        _context.Medicines.Where(a => a.Status == Enum.MedicineStatus.Active.ToString()).Select(m => new
                        {
                            MedicineId = m.MedicineId,
                            DisplayName = $"{m.GenericName} ({m.BrandName}) - {m.Form} {m.Strength.ToString("0.#")} {m.Unit}"
                        }),
                        "MedicineId",
                        "DisplayName",
                        model.MedicineId
                    );

                    TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
                    return View(model);
                }

                // ?? Fetch the existing medication order
                var order = await _context.MedicationOrders.FindAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Medication Order not found.";
                    return RedirectToAction("Index");
                }

                // ?? Update entity fields from the ViewModel
                order.PatientId = model.PatientId;
                order.MedicineId = model.MedicineId;
                order.UnitPerDose = model.UnitPerDose;
                order.Note = model.Note;
                order.ScheduledType = model.ScheduledType;
                order.DaysInterval = model.ScheduledType == "NonDaily" ? model.DaysInterval : null;
                order.Breakfast = model.Breakfast;
                order.Lunch = model.Lunch;
                order.Dinner = model.Dinner;
                order.Bedtime = model.Bedtime;
                order.StartDate = model.StartDate;
                order.DiscontinueDate = model.NoDiscontinueDate ? null : model.DiscontinueDate;
                order.NoDiscontinueDate = model.NoDiscontinueDate;

                // ?? Update audit fields
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = User.Identity?.Name ?? "System";

                // ?? Save changes to the database
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Medication order updated successfully!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                // ?? Handle database-specific errors
                Console.WriteLine($"Database error: {dbEx.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating the medication order. Please try again.";

                // Repopulate dropdowns on error
                ViewBag.PatientList = new SelectList(_context.Patients, "PatientId", "Firstname", model.PatientId);
                ViewBag.MedicineList = new SelectList(_context.Medicines, "MedicineId", "GenericName", model.MedicineId);

                return View(model);
            }
            catch (Exception ex)
            {
                // ?? Handle general errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please contact support.";

                // Repopulate dropdowns on error
                ViewBag.PatientList = new SelectList(_context.Patients, "PatientId", "Firstname", model.PatientId);
                ViewBag.MedicineList = new SelectList(_context.Medicines, "MedicineId", "GenericName", model.MedicineId);

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAdministrationLog(AdministrationLogViewModel model)
        {
            // Debug: raw form data
            Console.WriteLine("----- Raw Request.Form -----");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key} = {Request.Form[key]}");
            }
            Console.WriteLine("----- End Request.Form -----");

            // Remove Patient validation so it won't throw required error
            foreach (var key in ModelState.Keys.Where(k => k.Contains("Patient")).ToList())
                ModelState.Remove(key);

            if (model == null || model.Medications == null || !model.Medications.Any())
                return RedirectToAction(nameof(Index));

            // DEBUG: print incoming model values so you can inspect what was posted
            Console.WriteLine($"SaveAdministrationLog called. Medications.Count = {model?.Medications?.Count ?? 0}");
            if (model?.Medications != null)
            {
                foreach (var mi in model.Medications)
                {
                    Console.WriteLine($"Med Item -> MedicationOrderId={mi.MedicationOrderId}, PatientId={mi.PatientId}, BreakfastTaken={mi.BreakfastTaken}, LunchTaken={mi.LunchTaken}, DinnerTaken={mi.DinnerTaken}, BedtimeTaken={mi.BedtimeTaken}");
                }
            }

            var now = DateTime.Now;
            var dayStart = now.Date;
            var dayEnd = dayStart.AddDays(1);
            var currentUser = User?.Identity?.Name ?? "system";

            var logsToAdd = new List<AdministrationLog>();
            var billablesToAdd = new List<Billable>();
            var ordersToValidate = new HashSet<int>();

            foreach (var item in model.Medications)
            {
                ordersToValidate.Add(item.MedicationOrderId);

                var order = await _context.MedicationOrders
                                          .Include(o => o.Medicine)
                                          .FirstOrDefaultAsync(o => o.MedicationOrderId == item.MedicationOrderId);

                if (order == null)
                {
                    Console.WriteLine($"MedicationOrder not found for MedicationOrderId={item.MedicationOrderId}");
                    continue;
                }

                // try to find an existing administration log for this medication order today
                var existingLog = await _context.AdministrationLogs
                    .FirstOrDefaultAsync(a =>
                        a.MedicationOrderId == item.MedicationOrderId &&
                        a.AdministrationDate >= dayStart &&
                        a.AdministrationDate < dayEnd);

                if (existingLog != null)
                {
                    // DEBUG: show existing record values before update
                    Console.WriteLine($"Existing log found for OrderId={item.MedicationOrderId}. Before update -> Breakfast={existingLog.BreakfastTaken}, Lunch={existingLog.LunchTaken}, Dinner={existingLog.DinnerTaken}, Bedtime={existingLog.BedtimeTaken}");
                    // update existing record (do not insert new)
                    existingLog.BreakfastTaken = item.BreakfastTaken;
                    existingLog.LunchTaken = item.LunchTaken;
                    existingLog.DinnerTaken = item.DinnerTaken;
                    existingLog.BedtimeTaken = item.BedtimeTaken;
                    existingLog.RecordedBy = currentUser;
                    existingLog.AdministrationDate = now;
                    _context.AdministrationLogs.Update(existingLog);

                    // remove any medication billables for this order on the same day so we can recreate
                    var prevBillables = await _context.Billables
                        .Where(b => b.ReferenceId == item.MedicationOrderId
                                 && b.Category == "Medication"
                                 && b.DateAdded >= dayStart
                                 && b.DateAdded < dayEnd)
                        .ToListAsync();
                    if (prevBillables.Any())
                    {
                        Console.WriteLine($"Removing {prevBillables.Count} previous billable(s) for OrderId={item.MedicationOrderId}");
                        _context.Billables.RemoveRange(prevBillables);
                    }
                }
                else
                {
                    // DEBUG: creating new log
                    Console.WriteLine($"No existing log for OrderId={item.MedicationOrderId}. Creating new.");
                    // create new log
                    var log = new AdministrationLog
                    {
                        MedicationOrderId = item.MedicationOrderId,
                        PatientId = order.PatientId,
                        BreakfastTaken = item.BreakfastTaken,
                        LunchTaken = item.LunchTaken,
                        DinnerTaken = item.DinnerTaken,
                        BedtimeTaken = item.BedtimeTaken,
                        AdministrationDate = now,
                        RecordedBy = currentUser
                    };
                    logsToAdd.Add(log);
                }

                // prepare billables (we recreated after removing previous ones)
                var medicine = order.Medicine;
                decimal unitPrice = medicine?.Price ?? 0m;
                decimal quantity = order.UnitPerDose > 0 ? order.UnitPerDose : 1m;

                void AddBillable(string mealLabel)
                {
                    var desc = (medicine?.GenericName ?? "Medicine") + $" - {mealLabel}";
                    if (!string.IsNullOrWhiteSpace(medicine?.Unit))
                        desc += $" ({quantity} {medicine.Unit})";

                    billablesToAdd.Add(new Billable
                    {
                        PatientId = order.PatientId,
                        Category = "Medication",
                        Description = desc,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Amount = quantity * unitPrice,
                        DateAdded = now,
                        CreatedBy = currentUser,
                        ReferenceId = item.MedicationOrderId
                    });
                }

                if (item.BreakfastTaken) AddBillable("Breakfast");
                if (item.LunchTaken) AddBillable("Lunch");
                if (item.DinnerTaken) AddBillable("Dinner");
                if (item.BedtimeTaken) AddBillable("Bedtime");
            }

            if (logsToAdd.Any())
                await _context.AdministrationLogs.AddRangeAsync(logsToAdd);

            if (billablesToAdd.Any())
                await _context.Billables.AddRangeAsync(billablesToAdd);

            // Save changes (adds new logs/billables and applies updates/removals)
            await _context.SaveChangesAsync();

            // set ReferenceType after BillableId assigned for newly added billables
            if (billablesToAdd.Any())
            {
                foreach (var b in billablesToAdd)
                    b.ReferenceType = $"BILL-{b.BillableId:D5}";
                _context.Billables.UpdateRange(billablesToAdd);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        //This action will submit all temporary medication orders to the database and redirect to PatientProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrders()
        {
            var tempList = HttpContext.Session.GetObject<List<MedicationOrderViewModel>>(TempOrdersSessionKey)
                           ?? new List<MedicationOrderViewModel>();

            if (!tempList.Any())
            {
                TempData["ErrorMessage"] = "No medication orders to submit.";
                return RedirectToAction(nameof(AddMedicationOrder));
            }

            try
            {
                var toSave = new List<MedicationOrder>();

                foreach (var t in tempList)
                {
                    // Validate required foreign keys exist
                    var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == t.PatientId);
                    var medicineExists = await _context.Medicines.AnyAsync(m => m.MedicineId == t.MedicineId);

                    if (!patientExists || !medicineExists)
                    {
                        // skip invalid entry
                        continue;
                    }

                    string status;
                    if (t.StartDate.Date == DateTime.Today)
                        status = MedicationOrderStatus.Active.ToString();
                    else if (t.StartDate.Date > DateTime.Today)
                        status = MedicationOrderStatus.NotStarted.ToString();
                    else
                        status = MedicationOrderStatus.Active.ToString();

                    var entity = new MedicationOrder
                    {
                        PatientId = t.PatientId,
                        PsyProblemListId = t.PsyProblemListId,
                        MedicineId = t.MedicineId,
                        UnitPerDose = t.UnitPerDose,
                        Note = t.Note,
                        ScheduledType = t.ScheduledType,
                        DaysInterval = t.ScheduledType == "NonDaily" ? t.DaysInterval : null,
                        Breakfast = t.Breakfast,
                        Lunch = t.Lunch,
                        Dinner = t.Dinner,
                        Bedtime = t.Bedtime,
                        StartDate = t.StartDate,
                        DiscontinueDate = t.NoDiscontinueDate ? null : t.DiscontinueDate,
                        NoDiscontinueDate = t.NoDiscontinueDate,
                        CreatedAt = DateTime.Now,
                        CreatedBy = t.CreatedBy ?? User.Identity?.Name ?? "System",
                        Status = status
                    };

                    toSave.Add(entity);
                }

                if (toSave.Any())
                {
                    await _context.MedicationOrders.AddRangeAsync(toSave);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove(TempOrdersSessionKey);
                    TempData["SuccessMessage"] = "Medication orders submitted and saved.";
                    return RedirectToAction("Index", "PatientProfile", new { id = toSave.First().PatientId });
                }
                else
                {
                    TempData["ErrorMessage"] = "No valid medication orders to save.";
                    return RedirectToAction(nameof(AddMedicationOrder));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving medication orders: {ex}");
                TempData["ErrorMessage"] = "An error occurred while saving medication orders.";
                return RedirectToAction(nameof(AddMedicationOrder));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveTempOrder(int index)
        {
            var tempList = HttpContext.Session.GetObject<List<MedicationOrderViewModel>>(TempOrdersSessionKey)
                           ?? new List<MedicationOrderViewModel>();

            if (index >= 0 && index < tempList.Count)
            {
                tempList.RemoveAt(index);
                HttpContext.Session.SetObject(TempOrdersSessionKey, tempList);
                TempData["SuccessMessage"] = "Removed item from temporary list.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to remove item (invalid index).";
            }

            return RedirectToAction(nameof(AddMedicationOrder));
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string searchQuery = null, string status = null)
        {
            // Get all medicines
            var medicines = await _context.Medicines.ToListAsync();

            // Filter by search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowered = searchQuery.ToLower();
                medicines = medicines.Where(m =>
                    (!string.IsNullOrEmpty(m.GenericName) && m.GenericName.ToLower().Contains(lowered)) ||
                    (!string.IsNullOrEmpty(m.BrandName) && m.BrandName.ToLower().Contains(lowered))
                ).ToList();
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                medicines = medicines.Where(m => m.Status == status).ToList();
            }

            // Map to view model if needed
            var model = medicines.Select(m => new MedicineViewModel
            {
                MedicineId = m.MedicineId,
                GenericName = m.GenericName,
                BrandName = m.BrandName,
                Form = m.Form,
                Strength = m.Strength,
                Unit = m.Unit,
                Price = m.Price,
                Status = m.Status,
                DateAdded = m.DateAdded
            }).ToList();

            ViewBag.SearchQuery = searchQuery;
            ViewBag.Status = status;

            return View("Index", model);
        }
    }
}

