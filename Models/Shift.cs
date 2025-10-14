using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class Shift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string WorkerId { get; set; } = default!; // Using string UserId from Identity
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool IsClockedIn { get; set; }
        public DateTime? ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }

        // Navigation (optional)
        public virtual ICollection<ShiftChangeRequest>? ChangeRequests { get; set; }
    }
}
