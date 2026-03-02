using MediatR;

namespace ECommerce.Application.Products.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string ImageUrl,
    Guid CategoryId,
    bool IsActive) : IRequest<bool>;
