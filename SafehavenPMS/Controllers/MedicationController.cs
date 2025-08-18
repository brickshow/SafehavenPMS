using Microsoft.AspNetCore.Mvc;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.ViewModel;

namespace SafehavenPMS.Controllers
{
    public class MedicationController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public MedicationController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
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
                MedicineName = model.MedicineName,
                Form = model.Form,
                Dosage = model.Dosage,
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
    }
}
