using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EvergreenRanch.Models
{
    public class HealthCheck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HealthCheckId { get; set; }

        [Required]
        public int AnimalId { get; set; }
        [ForeignKey("AnimalId")]
        public Animal Animal { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public string VetUserId { get; set; }  

        public string Notes { get; set; }

        public bool Completed { get; set; } = false;

    }
}
