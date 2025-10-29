using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_backend.Models
{
    [Table("inventory")]
    public class Inventory
    {
        [Key]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}