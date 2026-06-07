using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Entities;
using ECommerce.Common.Result;

namespace ECommerce.BLL.Services
{
    /// <summary>
    /// Order Service: Business Logic for Order Processing
    /// 
    /// لماذا:
    /// - تحويل محتويات السلة إلى Order + OrderItems
    /// - حساب الإجمالي
    /// - تقليل المخزون (Stock)
    /// - مسح السلة بعد إتمام الطلب
    /// - استخدام Transaction لضمان الـ Atomicity
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<OrderDto>> PlaceOrderAsync(string userId, PlaceOrderRequest request)
        {
            try
            {
                // Step 1: Get user's cart
                var cartResult = await _unitOfWork.Carts.GetUserCartAsync(userId);
                if (cartResult.IsFailure || !cartResult.Data!.Any())
                    return Result<OrderDto>.Failure("Cart is empty");

                var cartItems = cartResult.Data;

                // Step 2: Start a transaction
                await _unitOfWork.BeginTransactionAsync();

                // Step 3: Validate stock for all items
                foreach (var item in cartItems)
                {
                    var productResult = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (productResult.IsFailure)
                        return Result<OrderDto>.Failure($"Product {item.ProductId} not found");

                    var product = productResult.Data!;
                    if (product.StockQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<OrderDto>.Failure($"Insufficient stock for product: {product.Name}. Available: {product.StockQuantity}");
                    }
                }

                // Step 4: Create the order
                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = GenerateOrderNumber(),
                    Status = "Pending",
                    ShippingAddress = request.ShippingAddress,
                    PaymentMethod = request.PaymentMethod,
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                // Step 5: Create order items and reduce stock
                foreach (var cartItem in cartItems)
                {
                    var productResult = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                    var product = productResult.Data!;

                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        Price = product.Price, // Snapshot of price at order time
                        Order = order
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += product.Price * cartItem.Quantity;

                    // Reduce stock
                    product.StockQuantity -= cartItem.Quantity;
                    await _unitOfWork.Products.UpdateAsync(product);
                }

                order.TotalAmount = totalAmount;
                order.OrderItems = order.OrderItems;

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Step 6: Clear cart
                await _unitOfWork.Carts.ClearUserCartAsync(userId);
                await _unitOfWork.SaveChangesAsync();

                // Step 7: Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                var orderDto = new OrderDto
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt
                };

                return Result<OrderDto>.Success(orderDto, "Order placed successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<OrderDto>.Failure($"Failed to place order: {ex.Message}");
            }
        }

        public async Task<Result<IReadOnlyList<OrderSummaryDto>>> GetUserOrdersAsync(string userId)
        {
            var ordersResult = await _unitOfWork.Orders.GetUserOrdersAsync(userId);
            if (ordersResult.IsFailure)
                return Result<IReadOnlyList<OrderSummaryDto>>.Failure(ordersResult.Message!);

            var summaries = ordersResult.Data!.Select(o => new OrderSummaryDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                ItemsCount = o.OrderItems?.Count ?? 0
            }).ToList();

            return Result<IReadOnlyList<OrderSummaryDto>>.Success(summaries);
        }

        public async Task<Result<OrderDetailsDto>> GetOrderDetailsAsync(string userId, int orderId)
        {
            var orderResult = await _unitOfWork.Orders.GetOrderWithDetailsAsync(userId, orderId);
            if (orderResult.IsFailure)
                return Result<OrderDetailsDto>.Failure(orderResult.Message!);

            var order = orderResult.Data!;

            var details = new OrderDetailsDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems?.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList() ?? new List<OrderItemDto>()
            };

            return Result<OrderDetailsDto>.Success(details);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
