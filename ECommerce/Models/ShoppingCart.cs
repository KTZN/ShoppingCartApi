namespace ECommerce.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool IsCheckedOut { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CheckedOutAt { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}
