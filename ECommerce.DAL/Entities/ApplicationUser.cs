using System;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.DAL.Entities
{
    /// <summary>
    /// ApplicationUser: extends IdentityUser to add custom fields
    /// Why: Microsoft Identity handles hashing, password validation, lockout, etc.
    /// We inherit from IdentityUser to get all of that for free
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Cart> CartItems { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
