using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_backend.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("promo_id")]
        public int? PromoId { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("status")]
        [Required]
        public string Status { get; set; } = "pending";

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("discount_amount")]
        public decimal DiscountAmount { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer? Customer { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(PromoId))]
        public virtual Promotion? Promotion { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}