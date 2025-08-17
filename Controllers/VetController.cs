using EvergreenRanch.Data;
using EvergreenRanch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class VetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VetController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Admin"))
            {
                var appointments = _context.HealthChecks
                    .Include(h => h.Animal)  // Include animal details
                    .OrderBy(h => h.ScheduledDate)
                    .ToList();
                return View(appointments);
            }
            else if (User.IsInRole("Worker"))
            {
                var workerAppointments = _context.HealthChecks
                    .Include(h => h.Animal)  // Include animal details
                    .Where(h => h.VetUserId == userId)
                    .OrderBy(h => h.ScheduledDate)
                    .ToList();
                return View(workerAppointments);
            }
            else
            {
                return Forbid();
            }
        }

        // STEP 1: Select date
        [HttpGet]
        public IActionResult SelectHealthDate(int animalId)
        {
            ViewBag.AnimalId = animalId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectHealthDate(int animalId, DateTime selectedDate)
        {
            if (selectedDate == DateTime.MinValue)
            {
                ModelState.AddModelError("", "Please select a valid date.");
                ViewBag.AnimalId = animalId;
                return View();
            }

            // Store date in session
            HttpContext.Session.SetDateTime("HealthCheckDate", selectedDate);
            HttpContext.Session.SetInt32("HealthCheckAnimalId", animalId);

            return RedirectToAction("SelectVet");
        }

        // STEP 2: Select vet
        [HttpGet]
        public async Task<IActionResult> SelectVet()
        {
            var selectedDate = HttpContext.Session.GetDateTime("HealthCheckDate");
            var animalId = HttpContext.Session.GetInt32("HealthCheckAnimalId");

            if (selectedDate == DateTime.MinValue || animalId == null)
                return RedirectToAction("Index", "Animals");

            // Get all vets by role
            var vets = await _userManager.GetUsersInRoleAsync("Worker");

            ViewBag.AnimalId = animalId.Value;
            ViewBag.SelectedDate = selectedDate;
            return View(vets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectVet(string vetId)
        {
            var selectedDate = HttpContext.Session.GetDateTime("HealthCheckDate");
            var animalId = HttpContext.Session.GetInt32("HealthCheckAnimalId");

            if (string.IsNullOrEmpty(vetId) || selectedDate == DateTime.MinValue || animalId == null)
                return RedirectToAction("Index", "Animals");

            var healthCheck = new HealthCheck
            {
                AnimalId = animalId.Value,
                ScheduledDate = selectedDate,
                VetUserId = vetId, // store vet UserId for now
                Notes = "Scheduled via system"
            };

            _context.HealthChecks.Add(healthCheck);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Health check scheduled successfully.";
            return RedirectToAction(nameof(BookingHealthSuccess));
        }

        public IActionResult BookingHealthSuccess()
        {
            return View();
        }

        // GET: Complete Health Check
        [HttpGet]
        public async Task<IActionResult> Complete(int id)
        {
            var healthCheck = await _context.HealthChecks
                .Include(h => h.Animal)
                .FirstOrDefaultAsync(h => h.HealthCheckId == id);

            if (healthCheck == null)
            {
                return NotFound();
            }

            // Authorization: Only the assigned vet can complete
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (healthCheck.VetUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Get vet's name for display
            var vetUser = await _userManager.FindByIdAsync(healthCheck.VetUserId);
            ViewBag.VetName = vetUser?.UserName ?? "Unknown Vet";

            return View(healthCheck);
        }

        // POST: Complete Health Check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, HealthCheck model)
        {
            var healthCheck = await _context.HealthChecks.FindAsync(id);
            if (healthCheck == null)
            {
                return NotFound();
            }

            // Authorization check
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (healthCheck.VetUserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Update only the fields we want to allow
            healthCheck.Notes = model.Notes;
            healthCheck.Completed = true;

            try
            {
                _context.Update(healthCheck);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Health check completed successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again.");
            }

            // Reload animal data if we need to show the form again
            healthCheck.Animal = await _context.Animals.FindAsync(healthCheck.AnimalId);
            return View(healthCheck);
        }


        // GET: HealthCheck Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var healthCheck = await _context.HealthChecks
                .Include(h => h.Animal)
                .FirstOrDefaultAsync(h => h.HealthCheckId == id);

            if (healthCheck == null)
            {
                return NotFound();
            }

            // Get vet's name for display
            var vetUser = await _userManager.FindByIdAsync(healthCheck.VetUserId);
            ViewBag.VetName = vetUser?.UserName ?? "Unknown Vet";

            return View(healthCheck);
        }
    }
}
