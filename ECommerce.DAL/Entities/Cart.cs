using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.DAL.Entities
{
    /// <summary>
    /// Cart Entity: represents an item in the user's shopping cart
    /// Why: we link the cart to a user (via UserId) and a product (via ProductId)
    /// </summary>
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
