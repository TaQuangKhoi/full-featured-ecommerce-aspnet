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

    public AdminOrdersController(ISender sender)
    {
        _sender = sender;
    }

    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        var orders = await _sender.Send(new GetOrdersQuery(PageSize: 50));
        return View(orders);
    }

    [Route("{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(id));
        if (order is null)
            return NotFound();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status)
    {
        await _sender.Send(new UpdateOrderStatusCommand(id, status));
        return RedirectToAction(nameof(Details), new { id });
    }
}
