using ECommerce.Domain.Entities;
using FluentAssertions;

namespace ECommerce.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Product_Creation_SetsProperties()
    {
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            Stock = 100,
            ImageUrl = "https://example.com/image.png",
            CategoryId = categoryId,
            IsActive = true
        };

        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("Test Description");
        product.Price.Should().Be(29.99m);
        product.Stock.Should().Be(100);
        product.ImageUrl.Should().Be("https://example.com/image.png");
        product.CategoryId.Should().Be(categoryId);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_DefaultValues_AreCorrect()
    {
        var product = new Product();

        product.IsActive.Should().BeTrue();
        product.Name.Should().BeEmpty();
        product.Description.Should().BeEmpty();
        product.ImageUrl.Should().BeEmpty();
        product.OrderItems.Should().NotBeNull().And.BeEmpty();
    }
}
