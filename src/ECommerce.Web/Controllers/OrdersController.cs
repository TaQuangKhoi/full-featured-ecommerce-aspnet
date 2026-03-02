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

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await _sender.Send(new GetOrdersQuery(CustomerId: customerId));
        return View(orders);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(id));
        if (order is null)
            return NotFound();

        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (order.CustomerId != customerId)
            return Forbid();

        return View(order);
    }
}
