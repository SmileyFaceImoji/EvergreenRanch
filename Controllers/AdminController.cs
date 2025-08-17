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
            ViewBag.AnimalTypes = Enum.GetValues<TypeAnimal>();
            ViewBag.HealthStatuses = Enum.GetValues<StatusHealth>();
            ViewBag.CurrentStatuses = Enum.GetValues<StatusAnimal>();
            ViewBag.Genders = Enum.GetNames<TypeGender>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAnimal(IFormCollection form)
        {
            try
            {
                byte[] pictureBytes = null;
                var file = form.Files["Picture"];
                if (file?.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        pictureBytes = ms.ToArray();
                    }
                }

                // Create new Animal object
                var animal = new Animal
                {
                    AnimalTag = form["AnimalTag"],
                    AnimalTypeID = Enum.Parse<TypeAnimal>(form["AnimalTypeID"]),
                    Gender = form["Gender"],
                    WeightKg = decimal.Parse(form["WeightKg"]),
                    AgeInMonths = int.Parse(form["AgeInMonths"]),
                    HealthStatus = Enum.Parse<StatusHealth>(form["HealthStatus"]),
                    CurrentStatus = Enum.Parse<StatusAnimal>(form["CurrentStatus"]),
                    Picture = pictureBytes,
                    MarketPrice = decimal.Parse(form["MarketPrice"])
                };

                _context.Animals.Add(animal);
                _context.SaveChanges();

                return RedirectToAction("","Purchase");  // Redirect to listing
            }
            catch (Exception ex)
            {
                // Repopulate dropdowns and return view with error
                ViewBag.AnimalTypes = Enum.GetValues<TypeAnimal>();
                ViewBag.HealthStatuses = Enum.GetValues<StatusHealth>();
                ViewBag.CurrentStatuses = Enum.GetValues<StatusAnimal>();
                ViewBag.Genders = Enum.GetNames<TypeGender>();

                ModelState.AddModelError("", $"Error: {ex.Message}");
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
