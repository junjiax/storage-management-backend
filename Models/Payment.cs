using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_backend.Models
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("payment_method")]
        public string PaymentMethod { get; set; } = "cash";

        [Required]
        [Column("payment_date")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!;
    }
}