using EvergreenRanch.Data;
using Microsoft.AspNetCore.Mvc;

namespace EvergreenRanch.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var animals = _context.Animals
                .Where(a => a.IsListedForSale)
                .ToList();

            return View(animals);
        }

        public IActionResult Details(int? id)
        {
            var TheAnimal = _context.Animals
                .Where(x => x.AnimalID == id)
                .FirstOrDefault();

            if (TheAnimal == null)
                return NotFound();

            return View(TheAnimal);
        }
    }
}
