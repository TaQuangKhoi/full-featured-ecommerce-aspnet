using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Orders.Commands;
using ECommerce.Application.Orders.Queries;
using ECommerce.Domain.Enums;

namespace ECommerce.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Area("Admin")]
[Route("Admin/Orders/[action]")]
public class AdminOrdersController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<AdminOrdersController> _logger;

    public AdminOrdersController(ISender sender, ILogger<AdminOrdersController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("[AdminOrders] Order list requested.");
        var orders = await _sender.Send(new GetOrdersQuery(PageSize: 50));
        _logger.LogInformation("[AdminOrders] Order list returned {Count} items.", orders.Items.Count);
        return View(orders);
    }

    [Route("{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        _logger.LogDebug("[AdminOrders] Order details requested for OrderId={OrderId}", id);

        var order = await _sender.Send(new GetOrderByIdQuery(id));
        if (order is null)
        {
            _logger.LogWarning("[AdminOrders] Order not found. OrderId={OrderId}", id);
            return NotFound();
        }

        _logger.LogInformation("[AdminOrders] Order details returned. OrderId={OrderId}, Status={Status}, CustomerId={CustomerId}",
            order.Id, order.Status, order.CustomerId);

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status)
    {
        _logger.LogInformation("[AdminOrders] Updating order status. OrderId={OrderId}, NewStatus={Status}", id, status);

        var success = await _sender.Send(new UpdateOrderStatusCommand(id, status));

        if (success)
            _logger.LogInformation("[AdminOrders] Order status updated successfully. OrderId={OrderId}, Status={Status}", id, status);
        else
            _logger.LogWarning("[AdminOrders] Order status update returned false (not found?). OrderId={OrderId}", id);

        return RedirectToAction(nameof(Details), new { id });
    }
}
