using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class Module
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public int SortOrder { get; set; }
        public Course? Course { get; set; }
    }
}
