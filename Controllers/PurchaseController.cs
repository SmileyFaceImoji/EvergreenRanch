using EvergreenRanch.Data;
using EvergreenRanch.Models.Common;
using EvergreenRanch.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cart;

        public PurchaseController(ApplicationDbContext context, CartService cart)
        {
            _context = context;
            _cart = cart;
        }
        public IActionResult Index()
        {
            var animals = _context.Animals.ToList();
            if (User.IsInRole("Admin"))
            {
                return View(animals);
            }
            else
            {
                var AnimalsForSale = animals
                    .Where(a => a.CurrentStatus == StatusAnimal.ForSale)
                    .ToList();

                return View(AnimalsForSale);
            }
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

        [HttpGet]
        public IActionResult MyCart()
        {
            var cartItemIds = _cart.GetCartItems();
            var animals = _context.Animals
                .Where(a => cartItemIds.Contains(a.AnimalID))
                .ToList();

            return View(animals);
        }


        // GET: Purchase/AddToCart?animalId=5
        public IActionResult AddToCart(int animalId)
        {
            var animal = _context.Animals.FirstOrDefault(a => a.AnimalID == animalId);

            if (animal == null || animal.CurrentStatus != StatusAnimal.ForSale)
            {
                TempData["ErrorMessage"] = "This animal is no longer available for purchase";
                return RedirectToAction("Details", new { id = animalId });
            }

            _cart.AddToCart(animalId);
            TempData["SuccessMessage"] = "Animal added to your cart!";

            // Redirect back to the details page
            return RedirectToAction(nameof(MyCart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int animalId)
        {
            _cart.RemoveFromCart(animalId);
            TempData["SuccessMessage"] = "Animal removed from cart";
            return RedirectToAction("MyCart");
        }
    }
}
