using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_backend.Models
{
    [Table("categories")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}