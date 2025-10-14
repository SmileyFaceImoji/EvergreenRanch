using EvergreenRanch.Data;
using EvergreenRanch.Models;
using EvergreenRanch.Models.Common;
using EvergreenRanch.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Controllers
{
    public class ShiftController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ShiftController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Worker's shifts
        public async Task<IActionResult> MyShifts()
        {
            var userId = _userManager.GetUserId(User);
            var shifts = await _context.Shifts
                .Where(s => s.WorkerId == userId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return View(shifts);
        }

        // POST: Clock in/out
        [HttpPost]
        public async Task<IActionResult> ClockInOut(ClockInOutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == model.ShiftId && s.WorkerId == userId);

            if (shift == null)
            {
                return NotFound();
            }

            if (model.Action == "in")
            {
                shift.IsClockedIn = true;
                shift.ClockInTime = DateTime.UtcNow;
            }
            else if (model.Action == "out")
            {
                shift.IsClockedIn = false;
                shift.ClockOutTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyShifts));
        }

        // GET: Request shift change
        public async Task<IActionResult> RequestChange(int id)
        {
            var userId = _userManager.GetUserId(User);
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == id && s.WorkerId == userId);

            if (shift == null)
            {
                return NotFound();
            }

            // Get available workers for swapping (excluding current user)
            var availableWorkers = await _userManager.Users
                .Where(u => u.Id != userId)
                .ToListAsync();

            var model = new RequestChangeViewModel
            {
                ShiftId = shift.Id,
                OriginalStartTime = shift.StartTime,
                OriginalEndTime = shift.EndTime,
                Location = shift.Location,
                AvailableWorkers = availableWorkers
            };

            return View(model);
        }

        // POST: Request shift change
        [HttpPost]
        public async Task<IActionResult> RequestChange(RequestChangeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload available workers if validation fails
                var serId = _userManager.GetUserId(User);
                model.AvailableWorkers = await _userManager.Users
                    .Where(u => u.Id != serId)
                    .ToListAsync();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            // Verify the shift still belongs to the user
            var shiftExists = await _context.Shifts
                .AnyAsync(s => s.Id == model.ShiftId && s.WorkerId == userId);

            if (!shiftExists)
            {
                return NotFound();
            }

            var request = new ShiftChangeRequest
            {
                ShiftId = model.ShiftId,
                RequestedByUserId = userId,
                RequestedWorkerId = model.RequestedWorkerId,
                RequestedStartTime = model.RequestedStartTime,
                RequestedEndTime = model.RequestedEndTime,
                Reason = model.Reason,
                Status = RequestStatus.Pending
            };

            _context.ShiftChangeRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Shift change request submitted successfully!";
            return RedirectToAction(nameof(MyShifts));
        }
        // GET: Admin - Pending requests
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingRequests()
        {
            var requests = await _context.ShiftChangeRequests
                .Include(r => r.Shift)
                .Where(r => r.Status == RequestStatus.Pending)
                .OrderBy(r => r.RequestedAt)
                .ToListAsync();

            var viewModel = new List<PendingRequestViewModel>();

            foreach (var request in requests)
            {
                var requester = await _userManager.FindByIdAsync(request.RequestedByUserId);
                var requestedWorker = !string.IsNullOrEmpty(request.RequestedWorkerId)
                    ? await _userManager.FindByIdAsync(request.RequestedWorkerId)
                    : null;

                viewModel.Add(new PendingRequestViewModel
                {
                    Request = request,
                    RequesterName = requester?.UserName ?? "Unknown User",
                    RequesterEmail = requester?.Email ?? "No Email",
                    RequestedWorkerName = requestedWorker?.UserName,
                    OriginalShiftInfo = $"{request.Shift.StartTime.ToString("f")} - {request.Shift.EndTime.ToString("t")} at {request.Shift.Location}"
                });
            }

            return View(viewModel);
        }
        // POST: Admin - Approve/Reject request
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRequest(int id, string action, string adminNotes)
        {
            var request = await _context.ShiftChangeRequests
                .Include(r => r.Shift)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            if (action == "approve")
            {
                request.Status = RequestStatus.Approved;

                // Update the shift if approved
                if (request.RequestedWorkerId != null)
                    request.Shift.WorkerId = request.RequestedWorkerId;

                if (request.RequestedStartTime.HasValue)
                    request.Shift.StartTime = request.RequestedStartTime.Value;

                if (request.RequestedEndTime.HasValue)
                    request.Shift.EndTime = request.RequestedEndTime.Value;
            }
            else if (action == "reject")
            {
                request.Status = RequestStatus.Rejected;
            }

            request.AdminNotes = adminNotes;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingRequests));
        }
    }
}
