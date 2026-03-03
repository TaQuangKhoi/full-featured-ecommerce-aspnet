using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Orders.Commands;

namespace ECommerce.Web.Controllers;

public class CartController : Controller
{
    private readonly ISender _sender;

    public CartController(ISender sender)
    {
        _sender = sender;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (customerId is null)
            return Unauthorized();

        var items = request.Items
            .Select(i => new OrderItemRequest(i.ProductId, i.Quantity))
            .ToList();

        var orderId = await _sender.Send(new CreateOrderCommand(
            CustomerId: customerId,
            ShippingAddress: request.ShippingAddress,
            Items: items));

        return Json(new { success = true, orderId });
    }
}

public class CheckoutRequest
{
    public string ShippingAddress { get; set; } = string.Empty;
    public List<CheckoutItemRequest> Items { get; set; } = new();
}

public class CheckoutItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
