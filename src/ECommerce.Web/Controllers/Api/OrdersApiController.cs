using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Orders.Commands;
using ECommerce.Application.Orders.Queries;
using ECommerce.Shared.DTOs;

namespace ECommerce.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<OrdersApiController> _logger;

    public OrdersApiController(ISender sender, ILogger<OrdersApiController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/orders — place a new order (checkout).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto dto)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("[OrdersApi] PlaceOrder called. CustomerId={CustomerId}, ItemCount={ItemCount}",
            customerId, dto.Items?.Count ?? 0);

        if (customerId is null)
        {
            _logger.LogWarning("[OrdersApi] CustomerId is null — user not authenticated.");
            return Unauthorized(new { success = false, message = "You must be logged in to place an order." });
        }

        if (string.IsNullOrWhiteSpace(dto.ShippingAddress))
        {
            _logger.LogWarning("[OrdersApi] ShippingAddress is empty.");
            return BadRequest(new { success = false, message = "Shipping address is required." });
        }

        if (dto.Items is null || dto.Items.Count == 0)
        {
            _logger.LogWarning("[OrdersApi] Items list is empty.");
            return BadRequest(new { success = false, message = "Cart is empty." });
        }

        var items = dto.Items
            .Select(i => new OrderItemRequest(i.ProductId, i.Quantity))
            .ToList();

        _logger.LogInformation("[OrdersApi] Sending CreateOrderCommand for CustomerId={CustomerId}", customerId);
        var orderId = await _sender.Send(new CreateOrderCommand(
            CustomerId: customerId,
            ShippingAddress: dto.ShippingAddress,
            Items: items));

        _logger.LogInformation("[OrdersApi] Order created. OrderId={OrderId}", orderId);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { success = true, orderId });
    }

    /// <summary>
    /// GET /api/orders — get the current user's orders.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogDebug("[OrdersApi] GetMyOrders. CustomerId={CustomerId}", customerId);

        var orders = await _sender.Send(new GetOrdersQuery(CustomerId: customerId));
        return Ok(orders);
    }

    /// <summary>
    /// GET /api/orders/{id} — get a specific order (only the owner may access it).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogDebug("[OrdersApi] GetById. OrderId={OrderId}, CustomerId={CustomerId}", id, customerId);

        var order = await _sender.Send(new GetOrderByIdQuery(id));
        if (order is null)
        {
            _logger.LogWarning("[OrdersApi] Order not found. OrderId={OrderId}", id);
            return NotFound(new { message = "Order not found." });
        }

        if (!string.Equals(order.CustomerId, customerId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[OrdersApi] Forbidden. OrderId={OrderId}, RequesterId={RequesterId}", id, customerId);
            return Forbid();
        }

        return Ok(order);
    }
}
