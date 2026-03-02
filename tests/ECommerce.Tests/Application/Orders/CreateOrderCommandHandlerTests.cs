using ECommerce.Application.Orders.Commands;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ECommerce.Tests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateOrderCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesOrder_WithCorrectTotal()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(productId1))
            .ReturnsAsync(new Product { Id = productId1, Name = "Product A", Price = 10.00m, Stock = 50 });
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(productId2))
            .ReturnsAsync(new Product { Id = productId2, Name = "Product B", Price = 25.00m, Stock = 30 });

        Order? capturedOrder = null;
        _unitOfWorkMock.Setup(u => u.Orders.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o)
            .ReturnsAsync((Order o) => o);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CreateOrderCommand(
            CustomerId: "customer-123",
            ShippingAddress: "456 Oak Ave",
            Items: new List<OrderItemRequest>
            {
                new(productId1, 2),  // 2 * 10.00 = 20.00
                new(productId2, 3)   // 3 * 25.00 = 75.00
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.Total.Should().Be(95.00m);
        _unitOfWorkMock.Verify(u => u.Orders.AddAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CreatesOrderItems()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(productId1))
            .ReturnsAsync(new Product { Id = productId1, Name = "Product A", Price = 15.00m, Stock = 50 });
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(productId2))
            .ReturnsAsync(new Product { Id = productId2, Name = "Product B", Price = 30.00m, Stock = 20 });

        Order? capturedOrder = null;
        _unitOfWorkMock.Setup(u => u.Orders.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o)
            .ReturnsAsync((Order o) => o);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CreateOrderCommand(
            CustomerId: "customer-456",
            ShippingAddress: "789 Pine St",
            Items: new List<OrderItemRequest>
            {
                new(productId1, 1),
                new(productId2, 4)
            });

        await _handler.Handle(command, CancellationToken.None);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.OrderItems.Should().HaveCount(2);

        var items = capturedOrder.OrderItems.ToList();
        items.Should().Contain(i => i.ProductId == productId1 && i.Quantity == 1 && i.UnitPrice == 15.00m);
        items.Should().Contain(i => i.ProductId == productId2 && i.Quantity == 4 && i.UnitPrice == 30.00m);
    }
}
