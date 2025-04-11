using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            if (await context.Users.AnyAsync())
            {
                return; // DB has been seeded
            }

            // Create admin user
            var admin = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            };
            context.Users.Add(admin);

            // Create sample products
            var products = new List<Product>
        {
            new Product { Name = "Laptop", Description = "High performance laptop", Price = 999.99m, StockQuantity = 10 },
            new Product { Name = "Smartphone", Description = "Latest smartphone", Price = 699.99m, StockQuantity = 20 },
            new Product { Name = "Headphones", Description = "Noise cancelling headphones", Price = 199.99m, StockQuantity = 30 },
            new Product { Name = "Keyboard", Description = "Mechanical keyboard", Price = 129.99m, StockQuantity = 15 },
            new Product { Name = "Mouse", Description = "Wireless mouse", Price = 49.99m, StockQuantity = 25 }
        };
            context.Products.AddRange(products);

            await context.SaveChangesAsync();
        }
    }
}
