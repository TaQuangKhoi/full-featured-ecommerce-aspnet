using ECommerce.Application.Products.Commands;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ECommerce.Tests.Application.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesProduct_ReturnsId()
    {
        _unitOfWorkMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) =>
            {
                p.Id = Guid.NewGuid();
                return p;
            });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CreateProductCommand(
            Name: "New Product",
            Description: "A new product",
            Price: 49.99m,
            Stock: 25,
            ImageUrl: "https://example.com/img.png",
            CategoryId: Guid.NewGuid(),
            IsActive: true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _unitOfWorkMock.Verify(u => u.Products.AddAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SetsCorrectProperties()
    {
        Product? capturedProduct = null;
        var categoryId = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .ReturnsAsync((Product p) => p);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CreateProductCommand(
            Name: "Widget",
            Description: "A fine widget",
            Price: 12.50m,
            Stock: 100,
            ImageUrl: "https://example.com/widget.png",
            CategoryId: categoryId,
            IsActive: false);

        await _handler.Handle(command, CancellationToken.None);

        capturedProduct.Should().NotBeNull();
        capturedProduct!.Name.Should().Be("Widget");
        capturedProduct.Description.Should().Be("A fine widget");
        capturedProduct.Price.Should().Be(12.50m);
        capturedProduct.Stock.Should().Be(100);
        capturedProduct.ImageUrl.Should().Be("https://example.com/widget.png");
        capturedProduct.CategoryId.Should().Be(categoryId);
        capturedProduct.IsActive.Should().BeFalse();
    }
}
