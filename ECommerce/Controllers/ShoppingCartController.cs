using ECommerce.DTOs;
using ECommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(IShoppingCartService shoppingCartService, ILogger<ShoppingCartController> logger)
        {
            _shoppingCartService = shoppingCartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var cart = await _shoppingCartService.GetOrCreateCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] CartItemDto itemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var cart = await _shoppingCartService.GetOrCreateCartAsync(userId);
                var item = await _shoppingCartService.AddItemToCartAsync(cart.Id, itemDto);

                return CreatedAtAction(nameof(GetCart), item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var cart = await _shoppingCartService.GetOrCreateCartAsync(userId);
                await _shoppingCartService.RemoveItemFromCartAsync(cart.Id, itemId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var cart = await _shoppingCartService.GetOrCreateCartAsync(userId);
                await _shoppingCartService.CheckoutCartAsync(cart.Id);

                return Ok(new { message = "Checkout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
