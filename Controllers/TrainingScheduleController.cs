using EvergreenRanch.Data;
using EvergreenRanch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class TrainingScheduleController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public TrainingScheduleController(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        // Admin view to schedule new training sessions
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var sessions = await _db.TrainingSessions
                .Include(t => t.Course)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return View(sessions);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new TrainingSession { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
        }

        [Authorize]
        public IActionResult Calendar()
        {
            var sessions = _db.TrainingSessions.ToList();
            return View(sessions);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(TrainingSession session)
        {
            if (!ModelState.IsValid) return View(session);

            _db.TrainingSessions.Add(session);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Training session scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Worker view of all available training sessions
        public async Task<IActionResult> Available()
        {
            var sessions = await _db.TrainingSessions
                .Where(t => t.StartDate >= DateTime.UtcNow)
                .Include(t => t.Course)
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            var userId = _um.GetUserId(User)!;
            var registered = await _db.TrainingRegistrations
                .Where(r => r.WorkerId == userId)
                .Select(r => r.TrainingSessionId)
                .ToListAsync();

            ViewBag.RegisteredIds = registered;
            return View(sessions);
        }
       


        [HttpPost]
        public async Task<IActionResult> Apply(int sessionId)
        {
            var userId = _um.GetUserId(User)!;

            bool already = await _db.TrainingRegistrations
                .AnyAsync(r => r.WorkerId == userId && r.TrainingSessionId == sessionId);
            if (already)
            {
                TempData["Message"] = "You have already applied for this training.";
                return RedirectToAction(nameof(Available));
            }

            var registration = new TrainingRegistration
            {
                WorkerId = userId,
                TrainingSessionId = sessionId
            };

            _db.TrainingRegistrations.Add(registration);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Training application submitted.";
            return RedirectToAction(nameof(Available));
        }
    }
}
