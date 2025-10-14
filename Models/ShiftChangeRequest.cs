using EvergreenRanch.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class ShiftChangeRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ShiftId { get; set; }
        public string RequestedByUserId { get; set; } = default!; // Worker requesting change
        public string? RequestedWorkerId { get; set; } // If swapping with someone
        public DateTime? RequestedStartTime { get; set; }
        public DateTime? RequestedEndTime { get; set; }
        public string Reason { get; set; } = default!;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public string? AdminNotes { get; set; }

        // Navigation
        public virtual Shift Shift { get; set; } = default!;
    }
}
