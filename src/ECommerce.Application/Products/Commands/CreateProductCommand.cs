using MediatR;

namespace ECommerce.Application.Products.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string ImageUrl,
    Guid CategoryId,
    bool IsActive = true) : IRequest<Guid>;
