using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto?>;
