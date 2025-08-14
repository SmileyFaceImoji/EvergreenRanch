using EvergreenRanch.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static EvergreenRanch.Models.Order;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Controllers
{
    
        [Authorize(Roles = "Driver,Admin")]
        public class DriversController : Controller
        {
            private readonly ApplicationDbContext _context;

            public DriversController(ApplicationDbContext context)
            {
                _context = context;
            }

            //list of deliveries
            [HttpGet]
            public async Task<IActionResult> Index()
            {
                var Orders = await _context.Orders.ToListAsync();

                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (User.IsInRole("Driver"))
                {
                    var DriverOrders = Orders.Where(x => x.DriverUserId == userid);
                    return View(DriverOrders);
                }
                else if (User.IsInRole("Admin"))
                {
                    return View(Orders);
                }
                else
                    return NotFound();
            }

            [HttpPost]
            public async Task<IActionResult> UpdateStatus(int orderId, StatusOrder status)
            {
                // Find the order
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound();
                }

                // Update status
                order.OrderStatus = status;

                // Set delivery timestamp if marked as delivered
                if (status == StatusOrder.Delivered)
                {
                    order.DeliveredAt = DateTime.UtcNow;
                }

                // Save changes
                await _context.SaveChangesAsync();

                // Success message
                TempData["SuccessMessage"] = $"Order #{orderId} status updated to {status}";

                // Redirect to delivery list
                return RedirectToAction(nameof(Index));
            }
        }
}
