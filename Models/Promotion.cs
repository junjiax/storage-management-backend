using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_backend.Models
{
    [Table("promotions")]
    public class Promotion
    {
        [Key]
        [Column("promo_id")]
        public int PromoId { get; set; }

        [Required]
        [Column("promo_code")]
        [StringLength(50)]
        public string PromoCode { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("discount_type")]
        public string DiscountType { get; set; } = string.Empty;

        [Required]
        [Column("discount_value")]
        public decimal DiscountValue { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("min_order_amount")]
        public decimal MinOrderAmount { get; set; }

        [Column("usage_limit")]
        public int UsageLimit { get; set; }

        [Column("used_count")]
        public int UsedCount { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "active";

        // Navigation property
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}