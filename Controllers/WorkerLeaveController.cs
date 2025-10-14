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
    public class WorkerLeaveController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public WorkerLeaveController(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        // Worker: View & apply for leave
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> MyLeaves()
        {
            var userId = _um.GetUserId(User)!;
            var leaves = await _db.WorkerLeaves
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.AppliedOn)
                .ToListAsync();
            return View(leaves);
        }

        [Authorize(Roles = "Worker")]
        public IActionResult Apply()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> Apply(WorkerLeave leave)
        {
            var userId = _um.GetUserId(User)!;

            // Validation: end must be after start
            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError("", "End date must be after start date.");
            }

            if (!ModelState.IsValid)
                return View(leave);

            leave.UserId = userId;
            leave.Status = LeaveStatus.Pending;
            leave.AppliedOn = DateTime.UtcNow;

            _db.WorkerLeaves.Add(leave);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Leave request submitted successfully.";
            return RedirectToAction(nameof(MyLeaves));
        }

        // Admin: View all leave requests
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var leaves = await _db.WorkerLeaves.Include(l => l.User)
                .OrderByDescending(l => l.AppliedOn)
                .ToListAsync();
            return View(leaves);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var leave = await _db.WorkerLeaves.FindAsync(id);
            if (leave == null) return NotFound();

            leave.Status = LeaveStatus.Approved;
            _db.WorkerLeaves.Update(leave);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Leave approved.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var leave = await _db.WorkerLeaves.FindAsync(id);
            if (leave == null) return NotFound();

            leave.Status = LeaveStatus.Rejected;
            _db.WorkerLeaves.Update(leave);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Leave rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
