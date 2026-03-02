using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
