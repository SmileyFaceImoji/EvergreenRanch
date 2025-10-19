using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EvergreenRanch.Models
{
    public class WorkerLeaveBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser? User { get; set; }

        // Tracks available leave balance
        [Range(0, 24)]
        public int AvailableDays { get; set; } = 0;

        // Last time days were updated (to handle monthly accrual)
        public DateTime LastAccrualDate { get; set; } = DateTime.UtcNow;
    }
}
