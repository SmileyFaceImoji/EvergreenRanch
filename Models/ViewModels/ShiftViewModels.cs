using Microsoft.AspNetCore.Identity;

namespace EvergreenRanch.Models.ViewModels
{
    public class ClockInOutViewModel
    {
        public int ShiftId { get; set; }
        public string Action { get; set; } = default!; // "in" or "out"
    }

    public class ShiftChangeRequestViewModel
    {
        public int ShiftId { get; set; }
        public DateTime? RequestedStartTime { get; set; }
        public DateTime? RequestedEndTime { get; set; }
        public string? RequestedWorkerId { get; set; }
        public string Reason { get; set; } = default!;
    }

    public class ShiftWithUserInfo
    {
        public Shift Shift { get; set; } = default!;
        public string WorkerName { get; set; } = default!;
        public string WorkerEmail { get; set; } = default!;
    }

    public class RequestChangeViewModel
    {
        public int ShiftId { get; set; }
        public DateTime OriginalStartTime { get; set; }
        public DateTime OriginalEndTime { get; set; }
        public string Location { get; set; } = default!;
        public DateTime? RequestedStartTime { get; set; }
        public DateTime? RequestedEndTime { get; set; }
        public string? RequestedWorkerId { get; set; }
        public string Reason { get; set; } = default!;
        public List<IdentityUser> AvailableWorkers { get; set; } = new(); // For dropdown
    }

    public class PendingRequestViewModel
    {
        public ShiftChangeRequest Request { get; set; } = default!;
        public string RequesterName { get; set; } = default!;
        public string RequesterEmail { get; set; } = default!;
        public string? RequestedWorkerName { get; set; }
        public string OriginalShiftInfo { get; set; } = default!;

    }

    public class ShiftSummaryViewModel
    {
        public string WorkerName { get; set; } = default!;
        public int TotalShifts { get; set; }
        public TimeSpan TotalHours { get; set; }
        public decimal TotalEarnings { get; set; }

    }
}

