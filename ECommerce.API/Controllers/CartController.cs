using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Helpers;
using ECommerce.BLL.Interfaces;
using ECommerce.Common.Helpers;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// CartController: Cart Management
    /// 
    /// Endpoints:
    /// GET    /api/cart           - Get user's cart
    /// POST   /api/cart           - Add to cart
    /// PUT    /api/cart           - Update cart item quantity
    /// DELETE /api/cart/{productId} - Remove from cart
    /// 
    /// IMPORTANT: UserId is extracted from JWT Claims (NOT passed in request body)
    /// </summary>
    [Authorize]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            // Extract UserId from JWT Claims (NOT from request body!)
            var userId = User.GetUserId();
            var result = await _cartService.GetUserCartAsync(userId);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartAddRequest request)
        {
            // Extract UserId from JWT Claims (NOT from request body!)
            var userId = User.GetUserId();
            var result = await _cartService.AddToCartAsync(userId, request);
            return HandleResult(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartQuantityRequest request)
        {
            var userId = User.GetUserId();
            var result = await _cartService.UpdateQuantityAsync(userId, request.ProductId, request.Quantity);
            return HandleResult(result);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, productId);
            return HandleResult(result);
        }
    }

    public class UpdateCartQuantityRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
