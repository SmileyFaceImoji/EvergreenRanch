using EvergreenRanch.Data;
using EvergreenRanch.Models;
using EvergreenRanch.Models.Common;
using EvergreenRanch.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            //Show list of animals available to sell(listed for sale, != sold, !dead
            return View();
        }

        [HttpGet]
        public IActionResult AddAnimal()
        {
            ViewBag.AnimalTypes = Enum.GetNames(typeof(TypeAnimal)).ToList();
            ViewBag.AnimalHealthStatuses = Enum.GetNames(typeof(StatusHealth)).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnimal(IFormCollection form)
        {
            try
            {
                var animal = new Animal
                {
                    AnimalTag = form["AnimalTag"],
                    AnimalTypeID = Enum.Parse<TypeAnimal>(form["AnimalTypeID"]),
                    Gender = form["Gender"],
                    WeightKg = decimal.Parse(form["WeightKg"]),
                    AgeInMonths = int.Parse(form["AgeInMonths"]),
                    HealthStatus = Enum.Parse<StatusHealth>(form["HealthStatus"]),
                    IsListedForSale = form["IsListedForSale"] == "on",
                    MarketPrice = decimal.Parse(form["MarketPrice"]),
                    DateAdded = DateTime.UtcNow
                };

                var file = Request.Form.Files["PictureFile"];
                if (file != null && file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    animal.Picture = ms.ToArray();
                }

                _context.Animals.Add(animal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to save: " + ex.Message);
                ViewBag.AnimalTypes = Enum.GetNames(typeof(TypeAnimal)).ToList();
                ViewBag.AnimalHealthStatuses = Enum.GetNames(typeof(StatusHealth)).ToList();
                return View();
            }
        }




        private IEnumerable<SelectListItem> GetAnimalTypeSelectList()
        {
            return Enum.GetValues(typeof(TypeAnimal))
                .Cast<TypeAnimal>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(), // must match the property binding!
                    Text = e.ToString()
                });
        }

        private IEnumerable<SelectListItem> GetHealthStatusSelectList()
        {
            return Enum.GetValues(typeof(StatusHealth))
                .Cast<StatusHealth>()
                .Select(h => new SelectListItem
                {
                    Value = h.ToString(), // use ToString() if binding to enum name
                    Text = h.ToString()
                });
        }



    }
}
