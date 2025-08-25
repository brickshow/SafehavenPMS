using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    public class MedicationController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public MedicationController(SafehavenPMSContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var medicines = await _context.Medicines.ToListAsync();

            var medicationOrders = await _context.MedicationOrders
                                            .Include(m => m.Patient)
                                            .Include(m => m.Medicine)
                                            .ToListAsync();

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
                    Status = m.Status
                }).ToList(),

                MedicationOrders = medicationOrders.Select(m => new MedicationOrderViewModel
                {
                    MedicationOrderId = m.MedicationOrderId,
                    PatientId = m.PatientId,
                    PatientName = m.Patient != null ? m.Patient.Firstname + " " + m.Patient.Lastname : "",
                    MedicineId = m.MedicineId,
                    MedicineName = m.Medicine != null ? m.Medicine.GenericName : "",
                    Dose = m.Dose,
                    Instruction = m.Instruction,
                    Frequency = m.Frequency,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate
                }).ToList()
            };

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
        public async Task<IActionResult> AddMedicationOrder(int? medicineId, int? patientId)
        {
            var medicines = await _context.Medicines.ToListAsync();
            var patients = await _context.Patients.ToListAsync();

            // Build SelectList for ViewBag
            ViewBag.PatientList = new SelectList(
                patients.Select(p => new {
                    PatientId = p.PatientId,
                    FullName = (p.Firstname ?? "") + " " + (p.Lastname ?? "")
                }),
                "PatientId",
                "FullName",
                patientId
            );

            var vm = new AddMedicationOrderViewModel
            {
                Medicines = medicines ?? new List<Medicine>(),
                SelectedMedicineId = medicineId,
                SelectedPatientId = patientId
            };

            if (medicineId.HasValue)
            {
                var med = medicines.FirstOrDefault(m => m.MedicineId == medicineId.Value);
                if (med != null)
                {
                    vm.Form = med.Form;
                    vm.Unit = med.Unit;
                }
            }

            return View(vm);
        }



        // ==========================
        // POST: Add Medication Order
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicationOrder(AddMedicationOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Log errors
                foreach (var entry in ModelState)
                    foreach (var error in entry.Value.Errors)
                        Console.WriteLine($"Field: {entry.Key} - Error: {error.ErrorMessage}");
                // Reload lists if validation fails
                vm.Medicines = await _context.Medicines.ToListAsync();
                vm.Patients = await _context.Patients.ToListAsync();
                return View(vm);
            }

            // Map ViewModel → MedicationOrder entity
            var order = new MedicationOrder
            {
                PatientId = vm.SelectedPatientId.Value,
                MedicineId = vm.SelectedMedicineId.Value,
                Dose = vm.Dose,
                Instruction = vm.Instruction,
                Frequency = vm.Frequency,
                StartDate = vm.StartDate.Value,
                EndDate = vm.EndDate.Value
            };

            _context.MedicationOrders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Medication");
        }



        //[HttpGet]
        //public async Task<IActionResult> SearchPatients(string term)
        //{
        //    var patients = await _context.Patients
        //        .Where(p => p.Firstname.Contains(term) || p.Lastname.Contains(term))
        //        .Select(p => new { id = p.PatientId, name = p.Firstname + " " + p.Lastname })
        //        .Take(10) // return top 10 matches
        //        .ToListAsync();

        //    return Json(patients);
        //}
    }
}
