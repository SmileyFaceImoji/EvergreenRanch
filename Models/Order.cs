using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EvergreenRanch.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        public string StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; } // Give this to stripe to trigger the refund process
        public decimal TotalAmount { get; set; }
        public StatusOrder OrderStatus { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        //Shipping informations
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string UserId { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); //individual animals
        public enum StatusOrder
        {
            Recieved,
            Prepping,
            Out,
            Delivered,
            HeldUp
        }

        public DateTime? DeliveredAt { get; set; }
        public string? DriverUserId { get; set; }


        //Customer confirming they recieved it(Migration is done)
        public required string SecretKey { get; set; }
        public bool RecievedByCustomer {  get; set; }
        public DateTime? RecievedAt { get; set; }
    }
}
