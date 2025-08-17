using EvergreenRanch.Data;
using EvergreenRanch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;
using EvergreenRanch.Utilities;
using EvergreenRanch.Models.ViewModels;
using EvergreenRanch.Models.Common;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class ReturnsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private const int MaxImages = 5;
        private const long MaxImageBytes = 2L * 1024 * 1024;

        public ReturnsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        //List of all requests
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var returns = await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.Animal)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            if (User.IsInRole("Admin"))
            {
                return View(returns);
            }
            else
            {
                var UserReturns = returns
                    .Where(x => x.UserId == userId);
                return View(UserReturns);
            }
        }

        public async Task<IActionResult> Create(int orderId, int animalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Animal)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order is null) return NotFound("Order not found");

            var animal = order.OrderItems.Select(oi => oi.Animal)
                .FirstOrDefault(a => a.AnimalID == animalId);
            if (animal is null) return NotFound("Animal not found in order");

            var vm = new ReturnRequestCreateVM
            {
                OrderId = orderId,
                AnimalId = animalId,
                UserId = userId
            };

            ViewBag.Animal = animal;
            ViewBag.Order = order;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReturnRequestCreateVM vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Why: ensure tampering didn't change IDs or ownership
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Animal)
                .FirstOrDefaultAsync(o => o.OrderId == vm.OrderId && o.UserId == userId);

            if (order is null)
            {
                ModelState.AddModelError("", "Order not found or not accessible.");
            }
            else if (!order.OrderItems.Any(oi => oi.AnimalID == vm.AnimalId))
            {
                ModelState.AddModelError("", "Animal not found in order.");
            }

            // Server-side image validation
            if (vm.EvidenceImages?.Count > MaxImages)
                ModelState.AddModelError(nameof(vm.EvidenceImages), $"Maximum {MaxImages} images allowed.");

            if (vm.EvidenceImages != null)
            {
                for (int i = 0; i < vm.EvidenceImages.Count; i++)
                {
                    var f = vm.EvidenceImages[i];
                    if (f.Length == 0)
                        ModelState.AddModelError($"{nameof(vm.EvidenceImages)}[{i}]", "Empty file.");
                    if (f.Length > MaxImageBytes)
                        ModelState.AddModelError($"{nameof(vm.EvidenceImages)}[{i}]", "File exceeds 2MB.");
                    if (string.IsNullOrWhiteSpace(f.ContentType) || !f.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        ModelState.AddModelError($"{nameof(vm.EvidenceImages)}[{i}]", "Only image files are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Re-hydrate sidebar data
                ViewBag.Order = order;
                ViewBag.Animal = order?.OrderItems.Select(oi => oi.Animal).FirstOrDefault(a => a.AnimalID == vm.AnimalId)
                                  ?? await _context.Animals.FindAsync(vm.AnimalId);
                return View(vm);
            }

            var entity = new ReturnRequests
            {
                OrderId = vm.OrderId,
                AnimalId = vm.AnimalId,
                UserId = userId,
                Reason = vm.Reason.Trim(),
                Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim(),
                Status = ReturnRequests.ReturnStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            if (vm.EvidenceImages != null && vm.EvidenceImages.Count > 0)
            {
                foreach (var file in vm.EvidenceImages)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    entity.Images.Add(new ReturnImage
                    {
                        ImageData = ms.ToArray(),
                        ContentType = file.ContentType
                    });
                }
            }

            _context.ReturnRequests.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Return request submitted successfully!";
            return RedirectToAction(nameof(Index));
        }

        //For admin to review request
        [Authorize(Roles = "Admin")]
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var theReturn = _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.Animal)
                .Include(r => r.Images)
                .FirstOrDefault(r => r.ReturnId == id);

            if (theReturn == null)
                return NotFound();

            return View(theReturn);
        }



        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.ReturnRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.ReturnId == id);

            if (request == null)
                return NotFound();

            if (string.IsNullOrEmpty(request.Order.StripePaymentIntentId))
            {
                ModelState.AddModelError("", "PaymentIntent ID missing — cannot refund.");
                return View("Details", request);
            }

            try
            {
                // Refund in Stripe
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = request.Order.StripePaymentIntentId
                    // You could add Amount = ... to refund partial payments
                };
                var refund = await refundService.CreateAsync(refundOptions);

                // Update status in DB
                request.Status = ReturnRequests.ReturnStatus.Refunded;
                request.ResolutionDate = DateTime.UtcNow;
                request.RefundAmount = request.Order.TotalAmount;

                // Optionally mark the animal as "NotForSale" again
                var animal = await _context.Animals.FindAsync(request.AnimalId);
                if (animal != null)
                {
                    animal.CurrentStatus = StatusAnimal.NotForSale;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Return approved and refund processed.";
                return RedirectToAction(nameof(Index));
            }
            catch (StripeException ex)
            {
                ModelState.AddModelError("", $"Stripe refund failed: {ex.Message}");
                return View("Details", request);
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int id)
        {
            var request = await _context.ReturnRequests
                .FirstOrDefaultAsync(r => r.ReturnId == id);

            if (request == null)
                return NotFound();

            request.Status = ReturnRequests.ReturnStatus.Rejected;
            request.ResolutionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Return request denied.";
            return RedirectToAction(nameof(Index));
        }
    }
}
