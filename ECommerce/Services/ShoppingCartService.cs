using ECommerce.Data;
using ECommerce.DTOs;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart> GetOrCreateCartAsync(int userId);
        Task<ShoppingCart> GetCartAsync(int cartId);
        Task<CartItem> AddItemToCartAsync(int cartId, CartItemDto itemDto);
        Task RemoveItemFromCartAsync(int cartId, int itemId);
        Task CheckoutCartAsync(int cartId);
    }

    public class ShoppingCartService : IShoppingCartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(AppDbContext context, ILogger<ShoppingCartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ShoppingCart> GetOrCreateCartAsync(int userId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(sc => sc.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(sc => sc.UserId == userId && !sc.IsCheckedOut);

                if (cart == null)
                {
                    cart = new ShoppingCart { UserId = userId };
                    _context.ShoppingCarts.Add(cart);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Created new cart for user {UserId}", userId);
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating cart for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ShoppingCart> GetCartAsync(int cartId)
        {
            try
            {
                return await _context.ShoppingCarts
                    .Include(sc => sc.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(sc => sc.Id == cartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart {CartId}", cartId);
                throw;
            }
        }

        public async Task<CartItem> AddItemToCartAsync(int cartId, CartItemDto itemDto)
        {
            try
            {
                var cart = await _context.ShoppingCarts.FindAsync(cartId);
                if (cart == null || cart.IsCheckedOut)
                {
                    throw new Exception("Cart not found or already checked out");
                }

                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new Exception("Product not found");
                }

                if (itemDto.Quantity <= 0)
                {
                    throw new Exception("Quantity must be greater than 0");
                }

                if (product.StockQuantity < itemDto.Quantity)
                {
                    throw new Exception("Not enough stock available");
                }

                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.ShoppingCartId == cartId && ci.ProductId == itemDto.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += itemDto.Quantity;
                }
                else
                {
                    existingItem = new CartItem
                    {
                        ShoppingCartId = cartId,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity
                    };
                    _context.CartItems.Add(existingItem);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Added item {ProductId} to cart {CartId}", itemDto.ProductId, cartId);

                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart {CartId}", cartId);
                throw;
            }
        }

        public async Task RemoveItemFromCartAsync(int cartId, int itemId)
        {
            try
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.ShoppingCartId == cartId);

                if (item == null)
                {
                    throw new Exception("Item not found in cart");
                }

                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed item {ItemId} from cart {CartId}", itemId, cartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ItemId} from cart {CartId}", itemId, cartId);
                throw;
            }
        }

        public async Task CheckoutCartAsync(int cartId)
        {
            try
            {
                var cart = await _context.ShoppingCarts
                    .Include(sc => sc.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(sc => sc.Id == cartId);

                if (cart == null)
                {
                    throw new Exception("Cart not found");
                }

                if (cart.IsCheckedOut)
                {
                    throw new Exception("Cart already checked out");
                }

                // Validate stock
                foreach (var item in cart.CartItems)
                {
                    if (item.Product.StockQuantity < item.Quantity)
                    {
                        throw new Exception($"Not enough stock for product {item.Product.Name}");
                    }
                }

                // Update stock
                foreach (var item in cart.CartItems)
                {
                    item.Product.StockQuantity -= item.Quantity;
                    item.Product.UpdatedAt = DateTime.UtcNow;
                }

                cart.IsCheckedOut = true;
                cart.CheckedOutAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Checked out cart {CartId}", cartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking out cart {CartId}", cartId);
                throw;
            }
        }
    }
}
