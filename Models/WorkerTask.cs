using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class WorkerTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        [Required]
        public string AssignedToUserId { get; set; } = default!;

        [ForeignKey(nameof(AssignedToUserId))]
        public virtual Microsoft.AspNetCore.Identity.IdentityUser? AssignedToUser { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime AssignedOn { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedOn { get; set; }
    }
}
