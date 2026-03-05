using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Orders.Commands;
using ECommerce.Application.Orders.Queries;
using ECommerce.Domain.Enums;

namespace ECommerce.Web.Controllers.Api;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/carts")]
public class AdminCartsApiController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<AdminCartsApiController> _logger;

    public AdminCartsApiController(ISender sender, ILogger<AdminCartsApiController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/admin/carts — get all orders (admin view of all user carts/orders).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        _logger.LogDebug("[AdminCartsApi] GetAll called. Page={Page}, PageSize={PageSize}", page, pageSize);

        var result = await _sender.Send(new GetOrdersQuery(Page: page, PageSize: pageSize));

        _logger.LogInformation("[AdminCartsApi] GetAll returned {Count} orders (page {Page}/{TotalPages}).",
            result.Items.Count, result.Page, result.TotalPages);

        return Ok(result);
    }

    /// <summary>
    /// PATCH /api/admin/carts/{id}/status — update an order's status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCartStatusDto dto)
    {
        _logger.LogInformation("[AdminCartsApi] UpdateStatus called. OrderId={OrderId}, NewStatus={Status}", id, dto.Status);

        if (!Enum.TryParse<OrderStatus>(dto.Status, ignoreCase: true, out var newStatus))
        {
            _logger.LogWarning("[AdminCartsApi] Invalid status value: {Status}", dto.Status);
            return BadRequest(new { error = $"Invalid status value: '{dto.Status}'." });
        }

        var success = await _sender.Send(new UpdateOrderStatusCommand(id, newStatus));

        if (!success)
        {
            _logger.LogWarning("[AdminCartsApi] Order not found. OrderId={OrderId}", id);
            return NotFound(new { error = "Order not found." });
        }

        _logger.LogInformation("[AdminCartsApi] Order status updated. OrderId={OrderId}, Status={Status}", id, newStatus);
        return NoContent();
    }
}

public record UpdateCartStatusDto(string Status);
