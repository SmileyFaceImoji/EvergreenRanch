using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class WorkerLeave
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public virtual Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public string Reason { get; set; } = default!;

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
    }
}
