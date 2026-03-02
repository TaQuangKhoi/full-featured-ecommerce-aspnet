using MediatR;

namespace ECommerce.Application.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
