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

        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            // Get currently logged-in worker
            var user = await _um.GetUserAsync(User);

            // Try to find their leave balance
            var balance = await _db.WorkerLeaveBalances
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            // If no record exists, assign default of 20 days
            int availableDays = balance?.AvailableDays ?? 20;

            var model = new WorkerLeaveApplyViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                AvailableDays = availableDays
            };

            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> Apply(WorkerLeaveApplyViewModel model)
        {
            if (model.EndDate < model.StartDate)
                ModelState.AddModelError("EndDate", "End date must be after start date.");

            if (model.StartDate < DateTime.Today)
                ModelState.AddModelError("StartDate", "Start date cannot be in the past.");

            if (!ModelState.IsValid)
                return View(model);

            var userId = _um.GetUserId(User)!;
            var balance = await GetOrUpdateLeaveBalanceAsync(userId);

            int requestedDays = (model.EndDate - model.StartDate).Days + 1;

            if (requestedDays > balance.AvailableDays)
            {
                TempData["Message"] = $"You only have {balance.AvailableDays} leave days available. Please adjust your request.";
                return RedirectToAction(nameof(MyLeaves));
            }

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

            TempData["Message"] = $"Leave request submitted successfully. You currently have {balance.AvailableDays} leave days remaining.";
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

            var balance = await _db.WorkerLeaveBalances.FirstOrDefaultAsync(b => b.UserId == leave.UserId);
            if (balance != null)
            {
                int days = (leave.EndDate - leave.StartDate).Days + 1;
                balance.AvailableDays = Math.Max(0, balance.AvailableDays - days);
                _db.WorkerLeaveBalances.Update(balance);
            }

            leave.Status = LeaveStatus.Approved;
            _db.WorkerLeaves.Update(leave);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Leave approved and deducted from balance.";
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
        private async Task<WorkerLeaveBalance> GetOrUpdateLeaveBalanceAsync(string userId)
        {
            var balance = await _db.WorkerLeaveBalances.FirstOrDefaultAsync(b => b.UserId == userId);

            if (balance == null)
            {
                balance = new WorkerLeaveBalance
                {
                    UserId = userId,
                    AvailableDays = 0,
                    LastAccrualDate = DateTime.UtcNow
                };
                _db.WorkerLeaveBalances.Add(balance);
                await _db.SaveChangesAsync();
            }

            // Calculate how many months since last accrual
            int monthsPassed = ((DateTime.UtcNow.Year - balance.LastAccrualDate.Year) * 12)
                                + (DateTime.UtcNow.Month - balance.LastAccrualDate.Month);

            if (monthsPassed > 0)
            {
                balance.AvailableDays = Math.Min(24, balance.AvailableDays + (2 * monthsPassed));
                balance.LastAccrualDate = DateTime.UtcNow;
                _db.WorkerLeaveBalances.Update(balance);
                await _db.SaveChangesAsync();
            }

            return balance;
        }

    }
}
