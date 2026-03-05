using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Products.Commands;
using ECommerce.Shared.DTOs;

namespace ECommerce.Web.Controllers.Api;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<ProductsApiController> _logger;

    public ProductsApiController(ISender sender, ILogger<ProductsApiController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (id != dto.Id)
        {
            _logger.LogWarning("[ProductsApi] Id mismatch: route id={RouteId}, body id={BodyId}", id, dto.Id);
            return BadRequest(new { error = "Id in route does not match Id in request body." });
        }

        _logger.LogInformation("[ProductsApi] Updating product ProductId={ProductId}, Name={Name}", id, dto.Name);

        var command = new UpdateProductCommand(
            Id: dto.Id,
            Name: dto.Name,
            Description: dto.Description,
            Price: dto.Price,
            Stock: dto.Stock,
            ImageUrl: dto.ImageUrl,
            CategoryId: dto.CategoryId,
            IsActive: dto.IsActive);

        var success = await _sender.Send(command);

        if (!success)
        {
            _logger.LogWarning("[ProductsApi] Product not found for update. ProductId={ProductId}", id);
            return NotFound(new { error = "Product not found." });
        }

        _logger.LogInformation("[ProductsApi] Product updated successfully. ProductId={ProductId}", id);
        return NoContent();
    }
}
