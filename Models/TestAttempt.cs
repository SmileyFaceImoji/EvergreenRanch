using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class TestAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public int CourseId { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }
        public DateTime AttemptedOn { get; set; } = DateTime.UtcNow;
    }
}
