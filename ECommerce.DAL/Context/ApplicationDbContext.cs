using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ECommerce.DAL.Entities;
using System.Reflection;

namespace ECommerce.DAL.Context
{
    /// <summary>
    /// ApplicationDbContext: Main EF Core DbContext
    /// Why: inherits from IdentityDbContext<ApplicationUser> to get all Identity tables
    /// (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.) plus our custom entities
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for our custom entities
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all configurations from the assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Unique index for OrderNumber
            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Unique constraint: User + Product in Cart (one item per user per product)
            builder.Entity<Cart>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();
        }
    }
}
