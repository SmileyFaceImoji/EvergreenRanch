using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class Certificate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CertificateGuid { get; set; } = Guid.NewGuid().ToString("N");
        public string UserId { get; set; } = default!;
        public int CourseId { get; set; }
        public DateTime IssuedOn { get; set; } = DateTime.UtcNow;
    }
}
