using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;
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
            // Fetch all medicines from the database
            var medicines = await _context.Medicines.ToListAsync();

            // Map to ViewModel
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
                    Price = m.Price
                }).ToList()
            };

            // Pass the ViewModel to the view
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


        [HttpGet]
        public async Task<IActionResult> AddMedicationOrder(int? medicineId)
        {
            // 1. Load all medicines and patients from the database
            var medicines = await _context.Medicines.ToListAsync();
            var patients = await _context.ClinicalStaffPatients.ToListAsync();

            // 2. Initialize the ViewModel with the lists and the currently selected medicineId
            var vm = new AddMedicationOrderViewModel
            {
                Medicines = medicines,
                Patients = patients,
                SelectedMedicineId = medicineId
            };

            // 3. If a medicine was selected (medicineId is not null)
            if (medicineId.HasValue)
            {
                // 3a. Find the medicine record that matches the selected ID
                var med = medicines.FirstOrDefault(m => m.MedicineId == medicineId.Value);

                // 3b. If found, populate the ViewModel with the medicine’s Form and Unit
                if (med != null)
                {
                    vm.Form = med.Form;
                    vm.Unit = med.Unit;
                }
            }

            // 4. Pass the ViewModel to the Razor view
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SearchPatients(string term)
        {
            var patients = await _context.Patients
                .Where(p => p.Firstname.Contains(term) || p.Lastname.Contains(term))
                .Select(p => new { id = p.PatientId, name = p.Firstname + " " + p.Lastname })
                .Take(10) // return top 10 matches
                .ToListAsync();

            return Json(patients);
        }
    }
}
