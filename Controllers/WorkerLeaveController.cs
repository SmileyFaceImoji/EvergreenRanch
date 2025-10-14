using EvergreenRanch.Data;
using EvergreenRanch.Models;
using EvergreenRanch.Models.Common;
using EvergreenRanch.Models.ViewModels;
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

        // GET: Apply for leave
        [Authorize(Roles = "Worker")]
        public IActionResult Apply()
        {
            var model = new WorkerLeaveApplyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };
            return View(model);
        }

        // POST: Apply for leave
        [HttpPost]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> Apply(WorkerLeaveApplyViewModel model)
        {
            // Custom validation: end date must be after start date
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            // Custom validation: start date cannot be in the past
            if (model.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _um.GetUserId(User)!;

            var leave = new WorkerLeave
            {
                UserId = userId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Reason = model.Reason,
                Status = LeaveStatus.Pending,
                AppliedOn = DateTime.UtcNow
            };

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
