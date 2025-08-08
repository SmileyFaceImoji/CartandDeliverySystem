using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartandDeliverySystem.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }         // Foreign key to Order
        public int ProductId { get; set; }       // Foreign key to Product
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // Price at time of purchase

        // Navigation properties
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
