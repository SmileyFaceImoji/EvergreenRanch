using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class TrainingSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Location { get; set; }

        // optional: link to a specific course
        public int? CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }

        // navigation for attendees
        public ICollection<TrainingRegistration> Registrations { get; set; } = new List<TrainingRegistration>();
    }

    public class TrainingRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainingSessionId { get; set; }

        [ForeignKey(nameof(TrainingSessionId))]
        public TrainingSession TrainingSession { get; set; } = default!;

        [Required]
        public string WorkerId { get; set; } = string.Empty;

        [ForeignKey(nameof(WorkerId))]
        public IdentityUser Worker { get; set; } = default!;

        public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

        public bool? IsApproved { get; set; } // optional
    }
}
