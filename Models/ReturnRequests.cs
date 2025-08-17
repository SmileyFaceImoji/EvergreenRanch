using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvergreenRanch.Models
{
    public class ReturnRequests
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReturnId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Reason for Return")]
        public string Reason { get; set; }

        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }

        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Status")]
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

        [Display(Name = "Resolution Date")]
        public DateTime? ResolutionDate { get; set; }

        [Display(Name = "Refund Amount")]
        public decimal? RefundAmount { get; set; }

        [Display(Name = "Stripe Refund ID")]
        public string? StripeRefundId { get; set; }
        [Display(Name = "Evidence Images")]
        public List<ReturnImage> Images { get; set; } = new List<ReturnImage>();

        public Order Order { get; set; }
        public Animal Animal { get; set; }

        public enum ReturnStatus
        {
            Pending,
            Approved,
            Rejected,
            Refunded
        }
    }

    public class ReturnImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageId { get; set; }

        [Required]
        public int ReturnId { get; set; }

        [Required]
        public byte[] ImageData { get; set; }

        [Required]
        public string ContentType { get; set; }

        public ReturnRequests ReturnRequests { get; set; }
    }
}
