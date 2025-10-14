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
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public TrainingController(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        // GET: /Training/Courses
        public async Task<IActionResult> Courses()
        {
            var courses = await _db.Courses.Include(c => c.Modules.OrderBy(m => m.SortOrder)).ToListAsync();
            return View(courses);
        }

        // GET: /Training/Modules/1
        public async Task<IActionResult> Modules(int courseId)
        {
            var userId = _um.GetUserId(User)!;
            var course = await _db.Courses.Include(c => c.Modules.OrderBy(m => m.SortOrder)).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            var progresses = await _db.UserModuleProgresses.Where(p => p.UserId == userId).ToListAsync();
            ViewBag.CompletedModuleIds = progresses.Where(p => p.IsCompleted).Select(p => p.ModuleId).ToHashSet();

            // check approved application for access
            var app = await _db.WorkerApplications.FirstOrDefaultAsync(w => w.UserId == userId && w.Status == ApplicationStatus.Approved);
            if (app == null)
            {
                ViewBag.CanAccess = false;
            }
            else
            {
                ViewBag.CanAccess = true;
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> MarkModuleComplete(int moduleId)
        {
            var userId = _um.GetUserId(User)!;
            var mod = await _db.Modules.FindAsync(moduleId);
            if (mod == null) return NotFound();

            var existing = await _db.UserModuleProgresses.FirstOrDefaultAsync(u => u.UserId == userId && u.ModuleId == moduleId);
            if (existing == null)
            {
                _db.UserModuleProgresses.Add(new UserModuleProgress { UserId = userId, ModuleId = moduleId, IsCompleted = true, CompletedOn = DateTime.UtcNow });
            }
            else
            {
                existing.IsCompleted = true;
                existing.CompletedOn = DateTime.UtcNow;
                _db.UserModuleProgresses.Update(existing);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Modules), new { courseId = mod.CourseId });
        }
    }
}
