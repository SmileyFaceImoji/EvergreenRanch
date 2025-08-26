using EvergreenRanch.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace EvergreenRanch.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<SelectListItem> DriverList { get; set; }
        public string AssignedDriver { get; set; }
    }
}
