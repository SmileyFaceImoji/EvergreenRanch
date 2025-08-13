using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EvergreenRanch.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }         // Foreign key to Order
        public int AnimalID { get; set; }       // Foreign key to Product
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // Price at time of purchase 

        // Navigation properties
        public Order Order { get; set; }
        public Animal Animal { get; set; }
    }
}
