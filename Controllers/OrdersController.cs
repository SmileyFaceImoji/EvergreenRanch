 using EvergreenRanch.Data;
using EvergreenRanch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var Orders = _context.Orders.ToList();
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Admin"))
            {
                return View(Orders);
            }
            else
            {
                var YourOrders = Orders
                    .Where(a=>a.UserId == userID)
                    .ToList();

                return View(YourOrders);
            }
        }

        public IActionResult Details(int? id)
        {
            if (id == null) 
                return View(nameof(Index));

            var ThisOrder = _context.Orders
                .Where(o => o.OrderId == id)
                .First();

            if (ThisOrder == null)
                return NotFound();

            return View(ThisOrder);
        }

        [HttpPost]
        public IActionResult DidYouGetIt(int OrderId, string SecretKey)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == OrderId);

            if (order == null)
                return NotFound();

            if (order.SecretKey == SecretKey)
            {
                order.RecievedByCustomer = true;
                order.RecievedAt = DateTime.UtcNow;

                _context.SaveChanges();

                // Redirect to Feedback form
                return RedirectToAction("Feedback", new { id = order.OrderId });
            }

            TempData["ErrorMessage"] = "Invalid Secret Key. Please try again.";
            return RedirectToAction("Details", new { id = OrderId });
        }

        [HttpGet]
        public IActionResult Feedback(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);

            if (order == null || !order.RecievedByCustomer)
                return RedirectToAction("Index");

            var model = new OrderFeedback { OrderId = id };
            return View(model);
        }

        [HttpPost]
        public IActionResult Feedback(OrderFeedback feedback)
        {
            if (!ModelState.IsValid)
                return View(feedback);

            _context.Add(feedback);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thank you for your feedback!";
            return RedirectToAction("Index");
        }


    }
}
