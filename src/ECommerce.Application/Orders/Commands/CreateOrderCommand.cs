using MediatR;

namespace ECommerce.Application.Orders.Commands;

public record CreateOrderCommand(
    string CustomerId,
    string ShippingAddress,
    List<OrderItemRequest> Items) : IRequest<Guid>;

public record OrderItemRequest(Guid ProductId, int Quantity);
