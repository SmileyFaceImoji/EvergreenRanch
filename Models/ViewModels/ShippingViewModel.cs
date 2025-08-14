using System.ComponentModel.DataAnnotations;

namespace EvergreenRanch.Models.ViewModels
{
    public class ShippingViewModel
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required, Display(Name = "Address")]
        public string Address { get; set; }

        [Required, Display(Name = "City")]
        public string City { get; set; }

        [Required, Display(Name = "Province")]
        public string Province { get; set; }

        [Required, Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
    }
}
