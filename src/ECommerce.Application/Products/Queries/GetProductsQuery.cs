using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Products.Queries;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    string? SearchTerm = null) : IRequest<PaginatedResult<ProductDto>>;
