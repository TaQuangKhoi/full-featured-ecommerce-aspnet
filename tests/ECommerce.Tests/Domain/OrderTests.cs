using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using FluentAssertions;

namespace ECommerce.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Order_Creation_SetsDefaults()
    {
        var order = new Order();

        order.Status.Should().Be(OrderStatus.Pending);
        order.CustomerId.Should().BeEmpty();
        order.ShippingAddress.Should().BeEmpty();
        order.OrderItems.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Order_AddOrderItem_UpdatesCollection()
    {
        var order = new Order
        {
            CustomerId = "customer-1",
            ShippingAddress = "123 Main St"
        };

        var orderItem = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            UnitPrice = 19.99m
        };

        order.OrderItems.Add(orderItem);

        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Quantity.Should().Be(2);
        order.OrderItems.First().UnitPrice.Should().Be(19.99m);
    }
}
