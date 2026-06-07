using System.ComponentModel.DataAnnotations;

namespace ECommerce.DAL.Entities
{
    /// <summary>
    /// Category Entity: represents a product category (e.g., Electronics, Clothing)
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
