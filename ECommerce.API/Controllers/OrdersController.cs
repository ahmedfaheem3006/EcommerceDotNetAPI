using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Helpers;
using ECommerce.BLL.Interfaces;
using ECommerce.Common.Helpers;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// OrdersController: Order Management
    /// 
    /// Endpoints:
    /// POST   /api/orders         - Place a new order (creates order from cart)
    /// GET    /api/orders         - Get user's order history
    /// GET    /api/orders/{id}    - Get order details
    /// 
    /// IMPORTANT: UserId is extracted from JWT Claims (NOT passed in request body)
    /// </summary>
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            // Extract UserId from JWT Claims (NOT from request body!)
            var userId = User.GetUserId();
            var result = await _orderService.PlaceOrderAsync(userId, request);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.GetUserId();
            var result = await _orderService.GetUserOrdersAsync(userId);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var userId = User.GetUserId();
            var result = await _orderService.GetOrderDetailsAsync(userId, id);
            return HandleResult(result);
        }
    }
}
