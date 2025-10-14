using EvergreenRanch.Data;
using EvergreenRanch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Controllers
{
    [Authorize]
    public class WorkerTasksController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public WorkerTasksController(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        // ADMIN: View and assign tasks
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var tasks = await _db.WorkerTasks.Include(t => t.AssignedToUser).ToListAsync();
            return View(tasks);
        }

        // ADMIN: Create Task
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Workers = await _um.GetUsersInRoleAsync("Worker");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(WorkerTask task)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Workers = await _um.GetUsersInRoleAsync("Worker");
                return View(task);
            }

            _db.WorkerTasks.Add(task);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Task assigned successfully.";
            return RedirectToAction(nameof(Index));
        }

        // WORKER: View their tasks
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> MyTasks()
        {
            var userId = _um.GetUserId(User)!;
            var tasks = await _db.WorkerTasks.Where(t => t.AssignedToUserId == userId).ToListAsync();
            return View(tasks);
        }

        // WORKER: Mark task done
        [HttpPost]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> MarkDone(int id)
        {
            var userId = _um.GetUserId(User)!;
            var task = await _db.WorkerTasks.FirstOrDefaultAsync(t => t.Id == id && t.AssignedToUserId == userId);

            if (task == null)
                return NotFound();

            task.IsCompleted = true;
            task.CompletedOn = DateTime.UtcNow;
            _db.Update(task);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Task marked as completed!";
            return RedirectToAction(nameof(MyTasks));
        }
    }
}
