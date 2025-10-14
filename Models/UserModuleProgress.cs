using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class UserModuleProgress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public int ModuleId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedOn { get; set; }
    }
}
