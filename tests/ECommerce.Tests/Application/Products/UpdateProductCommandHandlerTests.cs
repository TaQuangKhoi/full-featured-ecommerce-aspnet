using ECommerce.Application.Products.Commands;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ECommerce.Tests.Application.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateProductCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_UpdatesProduct_ReturnsTrue()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existing = new Product { Id = productId, Name = "Old Name", Price = 10m, Stock = 5 };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(productId)).ReturnsAsync(existing);
        _unitOfWorkMock.Setup(u => u.Products.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateProductCommand(
            Id: productId,
            Name: "Updated Name",
            Description: "Updated description",
            Price: 99.99m,
            Stock: 20,
            ImageUrl: "https://example.com/img.png",
            CategoryId: categoryId,
            IsActive: true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        existing.Name.Should().Be("Updated Name");
        existing.Description.Should().Be("Updated description");
        existing.Price.Should().Be(99.99m);
        existing.Stock.Should().Be(20);
        existing.ImageUrl.Should().Be("https://example.com/img.png");
        existing.CategoryId.Should().Be(categoryId);
        existing.IsActive.Should().BeTrue();

        _unitOfWorkMock.Verify(u => u.Products.UpdateAsync(existing), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFalse()
    {
        var productId = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand(
            Id: productId,
            Name: "Any Name",
            Description: string.Empty,
            Price: 1m,
            Stock: 0,
            ImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsActive: true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.Products.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_NullImageUrl_SetsEmptyString()
    {
        var productId = Guid.NewGuid();
        var existing = new Product { Id = productId, Name = "Product", ImageUrl = "old-url" };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(productId)).ReturnsAsync(existing);
        _unitOfWorkMock.Setup(u => u.Products.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new UpdateProductCommand(
            Id: productId,
            Name: "Product",
            Description: string.Empty,
            Price: 1m,
            Stock: 0,
            ImageUrl: null,
            CategoryId: Guid.NewGuid(),
            IsActive: true);

        await _handler.Handle(command, CancellationToken.None);

        existing.ImageUrl.Should().Be(string.Empty);
    }
}
