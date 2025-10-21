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

        [HttpPost]
        public async Task<IActionResult> ClockInOut(ClockInOutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == model.ShiftId && s.WorkerId == userId);

            if (shift == null)
                return NotFound();

            if (model.Action == "in")
            {
                shift.IsClockedIn = true;
                shift.ClockInTime = DateTime.UtcNow;

                // 🆕 Log attendance
                _context.ShiftAttendances.Add(new ShiftAttendance
                {
                    ShiftId = shift.Id,
                    WorkerId = userId,
                    ClockInTime = DateTime.UtcNow
                });
            }
            else if (model.Action == "out")
            {
                shift.IsClockedIn = false;
                shift.ClockOutTime = DateTime.UtcNow;

                var attendance = await _context.ShiftAttendances
                    .Where(a => a.ShiftId == shift.Id && a.WorkerId == userId && a.ClockOutTime == null)
                    .OrderByDescending(a => a.ClockInTime)
                    .FirstOrDefaultAsync();

                if (attendance != null)
                    attendance.ClockOutTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyShifts));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Summary()
        {
            var attendances = await _context.ShiftAttendances
                .Include(a => a.Shift)
                .ToListAsync();

            // Group by WorkerId and Month-Year
            var summary = attendances
                .GroupBy(a => new
                {
                    a.WorkerId,
                    Month = a.ClockInTime.Month,
                    Year = a.ClockInTime.Year
                })
                .Select(g => new
                {
                    WorkerId = g.Key.WorkerId,
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    TotalShifts = g.Count(),
                    TotalHours = g.Sum(a => a.TotalHours),
                    LateCount = g.Count(a => a.IsLate),
                    EarlyLeaves = g.Count(a => a.LeftEarly),
                    HourlyRate = g.Average(a => a.Shift.PayRate),
                    TotalPay = g.Sum(a => a.TotalHours * (double)a.Shift.PayRate)
                })
                .OrderBy(x => x.WorkerId)
                .ThenByDescending(x => x.Month)
                .ToList();

            // Fetch worker emails for all WorkerIds
            var workerIds = summary.Select(s => s.WorkerId).ToList();
            var workers = await _context.Users
                .Where(u => workerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email);

            // Map WorkerId -> Email
            var model = summary.Select(s => new
            {
                Worker = workers.ContainsKey(s.WorkerId) ? workers[s.WorkerId] : s.WorkerId,
                s.Month,
                s.TotalShifts,
                s.TotalHours,
                s.LateCount,
                s.EarlyLeaves,
                s.HourlyRate,
                s.TotalPay
            }).ToList();

            return View(model);
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
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRequest(int id, string action, string adminNotes)
        {
            var request = await _context.ShiftChangeRequests
                .Include(r => r.Shift)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (action == "approve")
            {
                // ✅ Overlap Prevention
                bool hasOverlap = await _context.Shifts
                    .AnyAsync(s => s.WorkerId == (request.RequestedWorkerId ?? request.Shift.WorkerId)
                                && s.Id != request.ShiftId
                                && ((request.RequestedStartTime ?? request.Shift.StartTime) < s.EndTime &&
                                    (request.RequestedEndTime ?? request.Shift.EndTime) > s.StartTime));

                if (hasOverlap)
                {
                    TempData["ErrorMessage"] = "Shift overlaps with an existing one for this worker!";
                    return RedirectToAction(nameof(PendingRequests));
                }

                // Approve request
                request.Status = RequestStatus.Approved;

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PayWorker(string workerId)
        {
            double hourlyRate = 120.0; // Example rate
            var totalHours = await _context.ShiftAttendances
                .Where(a => a.WorkerId == workerId)
                .SumAsync(a => a.TotalHours);

            var totalPay = Math.Round(totalHours * hourlyRate, 2);

            var payment = new Payment
            {
                WorkerId = workerId,
                TotalHours = totalHours,
                HourlyRate = hourlyRate,
                TotalPay = totalPay,
                PaymentDate = DateTime.Now,
                IsPaid = true
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment recorded successfully!";
            return RedirectToAction(nameof(Summary));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PaymentHistory()
        {
            var payments = await _context.Payments
                .Include(p => p.Worker)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }


    }
}
