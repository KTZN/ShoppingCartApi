using ECommerce.Data;
using ECommerce.DTOs;
using ECommerce.Models;
using ECommerce.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ECommerce.ECommerce.Tests
{
    public class ShoppingCartServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly Mock<ILogger<ShoppingCartService>> _loggerMock;

        public ShoppingCartServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _loggerMock = new Mock<ILogger<ShoppingCartService>>();
        }

        [Fact]
        public async Task GetOrCreateCartAsync_ShouldCreateNewCart_WhenNoActiveCartExists()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var service = new ShoppingCartService(context, _loggerMock.Object);
            var userId = 1;

            // Act
            var result = await service.GetOrCreateCartAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.False(result.IsCheckedOut);
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldAddNewItem_WhenItemNotInCart()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);

            // Seed data
            var product = new Product { Name = "Test Product", Price = 10.99m, StockQuantity = 5 };
            context.Products.Add(product);

            var cart = new ShoppingCart { UserId = 1 };
            context.ShoppingCarts.Add(cart);
            await context.SaveChangesAsync();

            var service = new ShoppingCartService(context, _loggerMock.Object);
            var itemDto = new CartItemDto { ProductId = product.Id, Quantity = 1 };

            // Act
            var result = await service.AddItemToCartAsync(cart.Id, itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.ProductId);
            Assert.Equal(1, result.Quantity);
        }

        [Fact]
        public async Task CheckoutCartAsync_ShouldMarkCartAsCheckedOut_WhenSuccessful()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);

            // Seed data
            var product = new Product { Name = "Test Product", Price = 10.99m, StockQuantity = 5 };
            context.Products.Add(product);

            var cart = new ShoppingCart { UserId = 1 };
            context.ShoppingCarts.Add(cart);

            var cartItem = new CartItem { ShoppingCartId = cart.Id, ProductId = product.Id, Quantity = 1 };
            context.CartItems.Add(cartItem);

            await context.SaveChangesAsync();

            var service = new ShoppingCartService(context, _loggerMock.Object);

            // Act
            await service.CheckoutCartAsync(cart.Id);

            // Assert
            var updatedCart = await context.ShoppingCarts.FindAsync(cart.Id);
            Assert.True(updatedCart.IsCheckedOut);
            Assert.NotNull(updatedCart.CheckedOutAt);

            var updatedProduct = await context.Products.FindAsync(product.Id);
            Assert.Equal(4, updatedProduct.StockQuantity); // 5 initial - 1 purchased
        }
    }
}
