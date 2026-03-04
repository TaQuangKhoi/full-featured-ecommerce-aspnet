using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Orders.Queries;

namespace ECommerce.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ISender sender, ILogger<OrdersController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogDebug("[Orders] Index requested for CustomerId={CustomerId}", customerId);

        var orders = await _sender.Send(new GetOrdersQuery(CustomerId: customerId));

        _logger.LogInformation("[Orders] Retrieved {Count} orders for CustomerId={CustomerId}",
            orders.Items.Count, customerId);

        return View(orders);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        _logger.LogDebug("[Orders] Details requested for OrderId={OrderId}", id);

        var order = await _sender.Send(new GetOrderByIdQuery(id));
        if (order is null)
        {
            _logger.LogWarning("[Orders] Order not found. OrderId={OrderId}", id);
            return NotFound();
        }

        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (order.CustomerId != customerId)
        {
            _logger.LogWarning("[Orders] Forbidden access. OrderId={OrderId}, RequesterId={RequesterId}, OwnerId={OwnerId}",
                id, customerId, order.CustomerId);
            return Forbid();
        }

        _logger.LogInformation("[Orders] Order details returned. OrderId={OrderId}, Status={Status}",
            order.Id, order.Status);

        return View(order);
    }
}
