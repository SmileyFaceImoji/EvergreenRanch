 using EvergreenRanch.Data;
using EvergreenRanch.Models;
using EvergreenRanch.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Frameworks;
using System.Security.Claims;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Index));

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            // Get all available drivers
            var drivers = await _userManager.GetUsersInRoleAsync("Driver");

            var driverList = drivers
                .Select(d => new SelectListItem
                {
                    Value = d.Id,
                    Text = d.Email
                })
                .ToList();

            // Get currently assigned driver (if any)
            string assignedDriver = string.Empty;
            if (!string.IsNullOrEmpty(order.DriverUserId))
            {
                var driverUser = await _userManager.FindByIdAsync(order.DriverUserId);
                assignedDriver = driverUser?.Email ?? "Unknown Driver";
            }

            var vm = new OrderDetailsViewModel
            {
                Order = order,
                DriverList = driverList,
                AssignedDriver = assignedDriver
            };

            return View(vm);
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AssignDriver(int orderId, string driverUserId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            order.DriverUserId = driverUserId;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Driver assigned successfully.";
            return RedirectToAction("Details", new { id = orderId });
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
        [ValidateAntiForgeryToken]
        public IActionResult Feedback(OrderFeedback feedback)
        {
            // Remove validation for Order nav property
            ModelState.Remove(nameof(feedback.Order));

            if (!ModelState.IsValid)
                return View(feedback);

            var newFeedback = new OrderFeedback
            {
                OrderId = feedback.OrderId,
                Rating = feedback.Rating,
                Comments = feedback.Comments,
                SubmittedAt = DateTime.UtcNow
            };

            _context.OrderFeedbacks.Add(newFeedback);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thank you for your feedback!";
            return RedirectToAction("Index");
        }



    }
}
