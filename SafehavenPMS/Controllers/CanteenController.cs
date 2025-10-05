using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Controllers
{
[Authorize]
    public class CanteenController : Controller
    {
        // GET: Canteen
        public IActionResult Index()
        {
            // TODO: Load list of canteen items/orders and pass to the view
            return View();
        }

        // // GET: Canteen/Details/5
        // public IActionResult Details(int? id)
        // {
        //     if (id == null)
        //         return NotFound();

        //     // TODO: Load the canteen item/order by id and pass to the view
        //     return View();
        // }

        // // GET: Canteen/Create
        // public IActionResult Create()
        // {
        //     // TODO: Prepare any data needed for the create view
        //     return View();
        // }

        // // POST: Canteen/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Create(/* add your model here, e.g. CanteenItem model */)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         // If validation fails, redisplay the form
        //         return View();
        //     }

        //     // TODO: Save the new canteen item/order
        //     return RedirectToAction(nameof(Index));
        // }

        // // GET: Canteen/Edit/5
        // public IActionResult Edit(int? id)
        // {
        //     if (id == null)
        //         return NotFound();

        //     // TODO: Load the item to edit and pass to the view
        //     return View();
        // }

        // // POST: Canteen/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Edit(int id /*, add your model here */)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return View();
        //     }

        //     // TODO: Update the canteen item/order
        //     return RedirectToAction(nameof(Index));
        // }

        // // GET: Canteen/Delete/5
        // public IActionResult Delete(int? id)
        // {
        //     if (id == null)
        //         return NotFound();

        //     // TODO: Load the item to confirm deletion
        //     return View();
        // }

        // // POST: Canteen/Delete/5
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public IActionResult DeleteConfirmed(int id)
        // {
        //     // TODO: Delete the canteen item/order
        //     return RedirectToAction(nameof(Index));
        // }
    }
}
