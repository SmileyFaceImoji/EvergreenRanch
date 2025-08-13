using EvergreenRanch.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EvergreenRanch.Models.ViewModels
{
    public class AddAnimalVM
    {
        [Required(ErrorMessage = "Tag is required")]
        public string AnimalTag { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public TypeAnimal AnimalType { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public TypeGender Gender { get; set; }

        [Range(0.1, 1000, ErrorMessage = "Weight must be between 0.1-1000 kg")]
        public decimal WeightKg { get; set; }

        [Range(1, 360, ErrorMessage = "Age must be 1-360 months")]
        public int AgeInMonths { get; set; }

        [Required(ErrorMessage = "Health status is required")]
        public StatusHealth HealthStatus { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public StatusAnimal CurrentStatus { get; set; }

        [Range(10, 100000, ErrorMessage = "Price must be $10-$100,000")]
        public decimal MarketPrice { get; set; }

        [Required(ErrorMessage = "Image is required")]
        [DataType(DataType.Upload)]
        public IFormFile ImageFile { get; set; }
    }
}
