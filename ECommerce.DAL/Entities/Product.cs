using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.DAL.Entities
{
    /// <summary>
    /// Product Entity: represents a product in the catalog
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Category Category { get; set; } = null!;
        public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
