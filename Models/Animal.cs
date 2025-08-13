using EvergreenRanch.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class Animal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnimalID { get; set; }
        public string AnimalTag { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public TypeAnimal AnimalTypeID { get; set; }  
        public string Gender { get; set; }
        public decimal WeightKg { get; set; }
        public int AgeInMonths { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public StatusHealth HealthStatus { get; set; }  

        [Column(TypeName = "nvarchar(24)")]
        public StatusAnimal CurrentStatus { get; set; }  
        public bool IsListedForSale { get; set; }
        public byte[] Picture { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public decimal MarketPrice { get; set; }
    }
}
