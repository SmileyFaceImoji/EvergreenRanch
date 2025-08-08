using EvergreenRanch.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EvergreenRanch.Models.ViewModels
{
    public class AddAnimalVM
    {
        [Required]
        public string AnimalTag { get; set; }

        [Required]
        public TypeAnimal AnimalTypeID { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public decimal WeightKg { get; set; }

        [Required]
        public int AgeInMonths { get; set; }

        public StatusHealth HealthStatus { get; set; }

        public StatusAnimal CurrentStatus { get; set; } = 0;

        public bool IsListedForSale { get; set; }

        public IFormFile PictureFile { get; set; }

        [Required]
        public decimal MarketPrice { get; set; }

        public IEnumerable<SelectListItem> AnimalTypes { get; set; }
        public IEnumerable<SelectListItem> AnimalHealthStatuses { get; set; }

    }
}
