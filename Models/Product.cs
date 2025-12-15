using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_backend.Models
{

  [Table("products")]
  public class Product
  {
    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("supplier_id")]
    public int? SupplierId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("product_name")]
    public string? ProductName { get; set; } = string.Empty;

    [StringLength(50)]
    [Column("barcode")]
    public string? Barcode { get; set; }

    [Required]
    [Column("price")]
    public decimal Price { get; set; }

    [StringLength(20)]
    [Column("unit")]
    public string? Unit { get; set; } = "pcs";

    [Column("product_img")]
    public string? ProductImg { get; set; } = string.Empty;

    [Column("product_public_id")]
    public string? ProductPublicId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public virtual Supplier? Supplier { get; set; }

    public virtual Inventory? Inventory { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
  }
}