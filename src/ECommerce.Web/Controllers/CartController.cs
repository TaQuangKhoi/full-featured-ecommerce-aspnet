using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Orders.Commands;

namespace ECommerce.Web.Controllers;

public class CartController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<CartController> _logger;

    public CartController(ISender sender, ILogger<CartController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        _logger.LogInformation("[Checkout] Request received. IsAuthenticated={IsAuthenticated}, ContentType={ContentType}",
            User.Identity?.IsAuthenticated, Request.ContentType);

        try
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("[Checkout] CustomerId={CustomerId}", customerId);

            if (customerId is null)
            {
                _logger.LogWarning("[Checkout] CustomerId is null — user not authenticated.");
                return Json(new { success = false, message = "You must be logged in to place an order." });
            }

            if (request is null)
            {
                _logger.LogWarning("[Checkout] Request body is null — possible JSON deserialization failure.");
                return Json(new { success = false, message = "Invalid request body." });
            }

            _logger.LogInformation("[Checkout] ShippingAddress={ShippingAddress}, ItemCount={ItemCount}",
                request.ShippingAddress, request.Items?.Count ?? 0);

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            {
                _logger.LogWarning("[Checkout] ShippingAddress is empty.");
                return Json(new { success = false, message = "Shipping address is required." });
            }

            if (request.Items is null || request.Items.Count == 0)
            {
                _logger.LogWarning("[Checkout] Items list is empty.");
                return Json(new { success = false, message = "Cart is empty." });
            }

            var items = request.Items
                .Select(i => new OrderItemRequest(i.ProductId, i.Quantity))
                .ToList();

            foreach (var item in items)
            {
                _logger.LogInformation("[Checkout] Item: ProductId={ProductId}, Quantity={Quantity}",
                    item.ProductId, item.Quantity);
            }

            _logger.LogInformation("[Checkout] Sending CreateOrderCommand...");
            var orderId = await _sender.Send(new CreateOrderCommand(
                CustomerId: customerId,
                ShippingAddress: request.ShippingAddress,
                Items: items));

            _logger.LogInformation("[Checkout] Order created successfully. OrderId={OrderId}", orderId);
            return Json(new { success = true, orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Checkout] Unhandled exception while processing checkout.");
            return Json(new { success = false, message = $"Server error: {ex.Message}" });
        }
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
