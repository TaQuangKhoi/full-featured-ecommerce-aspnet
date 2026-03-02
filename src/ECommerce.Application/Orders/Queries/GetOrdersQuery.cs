using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Orders.Queries;

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 10,
    string? CustomerId = null) : IRequest<PaginatedResult<OrderDto>>;
