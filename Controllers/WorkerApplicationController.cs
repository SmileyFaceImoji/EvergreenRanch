using EvergreenRanch.Data;
using EvergreenRanch.Models;
using EvergreenRanch.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class WorkerApplicationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public WorkerApplicationController(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        // GET: /WorkerApplication/Apply
        public async Task<IActionResult> Apply()
        {
            var userId = _um.GetUserId(User)!;
            var existing = await _db.WorkerApplications.FirstOrDefaultAsync(w => w.UserId == userId);
            return View(existing ?? new WorkerApplication());
        }

        [HttpPost]
        public async Task<IActionResult> ApplyPost(string? notes)
        {
            var userId = _um.GetUserId(User)!;
            var existing = await _db.WorkerApplications.FirstOrDefaultAsync(w => w.UserId == userId);
            if (existing != null)
            {
                if (existing.Status == ApplicationStatus.Pending)
                {
                    TempData["Message"] = "Your application is pending.";
                    return RedirectToAction(nameof(Apply));
                }
                existing.Notes = notes;
                existing.AppliedOn = DateTime.UtcNow;
                existing.Status = ApplicationStatus.Pending;
                _db.WorkerApplications.Update(existing);
            }
            else
            {
                _db.WorkerApplications.Add(new WorkerApplication { UserId = userId, Notes = notes, Status = ApplicationStatus.Pending });
            }
            await _db.SaveChangesAsync();
            TempData["Message"] = "Application submitted.";
            return RedirectToAction(nameof(Apply));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var list = await _db.WorkerApplications.Where(w => w.Status == ApplicationStatus.Pending).ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var app = await _db.WorkerApplications.FindAsync(id);
            if (app == null) return NotFound();
            app.Status = ApplicationStatus.Approved;
            _db.WorkerApplications.Update(app);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Application approved.";
            return RedirectToAction(nameof(Pending));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var app = await _db.WorkerApplications.FindAsync(id);
            if (app == null) return NotFound();
            app.Status = ApplicationStatus.Rejected;
            _db.WorkerApplications.Update(app);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Application rejected.";
            return RedirectToAction(nameof(Pending));
        }

        public async Task<IActionResult> MyApplications()
        {
            var userId = _um.GetUserId(User)!;
            var apps = await _db.WorkerApplications.Where(w => w.UserId == userId).ToListAsync();
            return View(apps);
        }
    }
}
