﻿namespace ECommerce.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
