using Microsoft.AspNetCore.Mvc;

namespace SafehavenPMS.Controllers
{
    public class MedicalHistoryController : Controller
    {
        // GET: MedicalHistory
        public IActionResult _MedicalHistory()
        {
            return PartialView("_MedicalHistory");
        }

        // // GET: MedicalHistory/Details/5
        // public IActionResult Details(int id)
        // {
        //     return View();
        // }

        // // GET: MedicalHistory/Create
        // public IActionResult Create()
        // {
        //     return View();
        // }

        // // POST: MedicalHistory/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Create(/* Add parameters here */)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         // Add creation logic herez
        //     }
        //     return View();
        // }

        // // GET: MedicalHistory/Edit/5
        // public IActionResult Edit(int id)
        // {
        //     return View();
        // }

        // // POST: MedicalHistory/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Edit(int id /*, Add parameters here */)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         // Add update logic here
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View();
        // }

        // // GET: MedicalHistory/Delete/5
        // public IActionResult Delete(int id)
        // {
        //     return View();
        // }

        // // POST: MedicalHistory/Delete/5
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public IActionResult DeleteConfirmed(int id)
        // {
        //     // Add delete logic here
        //     return RedirectToAction(nameof(Index));
        // }
    }
}