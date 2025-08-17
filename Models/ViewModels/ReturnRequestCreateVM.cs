using System.ComponentModel.DataAnnotations;

namespace EvergreenRanch.Models.ViewModels
{
    public class ReturnRequestCreateVM
    {
        [Required] public int OrderId { get; set; }
        [Required] public int AnimalId { get; set; }
        [Required] public string UserId { get; set; } = default!;

        [Required, Display(Name = "Reason for Return")]
        [StringLength(4000)]
        public string Reason { get; set; } = default!;

        [Display(Name = "Additional Information")]
        [StringLength(4000)]
        public string? Notes { get; set; }

        // Must match input name in the form for model binding.
        [Display(Name = "Evidence Images")]
        public List<IFormFile> EvidenceImages { get; set; } = new();
    }
}
