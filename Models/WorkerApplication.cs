using EvergreenRanch.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class WorkerApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        // New fields
        public string? IdDocumentPath { get; set; }
        public string? CvPath { get; set; }
        public string? CoverLetterPath { get; set; } 
    }
}
