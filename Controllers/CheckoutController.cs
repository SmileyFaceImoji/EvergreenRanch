using EvergreenRanch.Data;
using EvergreenRanch.Models;
using EvergreenRanch.Models.Common;
using EvergreenRanch.Models.ViewModels;
using EvergreenRanch.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using EvergreenRanch.Utilities;


namespace EvergreenRanch.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cart;
        private readonly string _stripeSecretKey;
        private const decimal TaxRate = 0.15m;

        public CheckoutController(ApplicationDbContext context, CartService cart, IConfiguration configuration)
        {
            _context = context;
            _cart = cart;
            _stripeSecretKey = configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        [HttpGet]
        public IActionResult ConfirmAddress()
        {
            var model = new ShippingViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmAddress(ShippingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Save shipping info to session
            HttpContext.Session.SetObject("ShippingInfo", model);

            return RedirectToAction("Pay");
        }

        public IActionResult Pay()
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;
            //getting the cart in its session
            // Get cart items
            var cartItemIds = _cart.GetCartItems();
            var animals = _context.Animals
                .Where(a => cartItemIds.Contains(a.AnimalID))
                .ToList();
            if (!animals.Any()) return RedirectToAction("Index", "Home");
            //getting shipping details in its session
            // Get shipping info from session
            var shippingInfo = HttpContext.Session.GetObject<ShippingViewModel>("ShippingInfo");
            if (shippingInfo == null) return RedirectToAction("ConfirmAddress");

            // Calculate totals
            var subtotal = animals.Sum(a => a.MarketPrice);
            var tax = subtotal * TaxRate;
            var total = subtotal + tax;

            // Create Stripe session
            var domain = "https://localhost:7009";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = animals.Select(animal => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(animal.MarketPrice * 100), // convert to cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{animal.AnimalTag} ({animal.AnimalTypeID})",
                            Description = $"{animal.Gender} • {animal.WeightKg}kg • {animal.AgeInMonths} months",
                            //Images = animal.Picture != null ?
                            //    new List<string> { $"data:image/jpeg;base64,{Convert.ToBase64String(animal.Picture)}" } :
                            //    null
                        }
                    },
                    Quantity = 1,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + "/Checkout/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/Checkout/Cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", User.FindFirstValue(ClaimTypes.NameIdentifier) },
                    { "shipping_name", shippingInfo.FullName },
                    { "shipping_address", $"{shippingInfo.Address}, {shippingInfo.City}, {shippingInfo.Province} {shippingInfo.PostalCode}" }
                }
            };

            // Add tax as a separate line item
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(tax * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Tax (15%)",
                        Description = "Sales tax"
                    }
                },
                Quantity = 1,
            });

            var service = new SessionService();
            var session = service.Create(options);


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(userId);

             

            return Redirect(session.Url);
        }

        public IActionResult Success(string session_id)
        {
            // Verify the session
            var sessionService = new SessionService();
            var session = sessionService.Get(session_id);

            // Check if session is valid and paid
            if (session.PaymentStatus != "paid")
            {
                return RedirectToAction("Cancel");
            }

            var paymentIntentId = session.PaymentIntentId;

            // Get cart items
            var cartItemIds = _cart.GetCartItems();
            var animals = _context.Animals
                .Where(a => cartItemIds.Contains(a.AnimalID))
                .ToList();

            // Update animal status to sold
            foreach (var animal in animals)
            {
                animal.CurrentStatus = StatusAnimal.Sold;
            }

            // Get shipping info from session
            var shippingInfo = HttpContext.Session.GetObject<ShippingViewModel>("ShippingInfo");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Create order record
            var order = new Order
            {
                StripeSessionId = session_id,
                StripePaymentIntentId = paymentIntentId,
                TotalAmount = animals.Sum(a => a.MarketPrice) * (1 + TaxRate),
                UserId = userId,
                FullName = shippingInfo.FullName,
                Address = shippingInfo.Address,
                City = shippingInfo.City,
                Province = shippingInfo.Province,
                PostalCode = shippingInfo.PostalCode,
                OrderStatus = Order.StatusOrder.Recieved,
                OrderItems = animals.Select(a => new OrderItem
                {
                    AnimalID = a.AnimalID,
                    UnitPrice = a.MarketPrice
                }).ToList(),
                SecretKey = RandomisorExtension.GenerateRandom(8, RandomCharType.Uppercase | RandomCharType.Lowercase | RandomCharType.Digit)
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Clear cart and session data
            _cart.ClearCart();
            HttpContext.Session.Remove("ShippingInfo");
            HttpContext.Session.Remove("StripeSessionId");

            return RedirectToAction("","Orders");
        }

        public IActionResult Cancel() => View();
    }
}
