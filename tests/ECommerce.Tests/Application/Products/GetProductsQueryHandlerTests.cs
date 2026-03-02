using System.Linq.Expressions;
using AutoMapper;
using ECommerce.Application.Mappings;
using ECommerce.Application.Products.Queries;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ECommerce.Tests.Application.Products;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new GetProductsQueryHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ReturnsProducts_WhenProductsExist()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Price = 10m, Stock = 5 },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Price = 20m, Stock = 10 }
        };

        _unitOfWorkMock.Setup(u => u.Products.GetAllAsync())
            .ReturnsAsync(products);

        var query = new GetProductsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedResults()
    {
        var products = Enumerable.Range(1, 15)
            .Select(i => new Product { Id = Guid.NewGuid(), Name = $"Product {i}", Price = i * 10m, Stock = i })
            .ToList();

        _unitOfWorkMock.Setup(u => u.Products.GetAllAsync())
            .ReturnsAsync(products);

        var query = new GetProductsQuery(Page: 2, PageSize: 5);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_FiltersBy_CategoryId()
    {
        var categoryId = Guid.NewGuid();
        var filteredProducts = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Filtered Product", Price = 15m, Stock = 3, CategoryId = categoryId }
        };

        _unitOfWorkMock.Setup(u => u.Products.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(filteredProducts);

        var query = new GetProductsQuery(CategoryId: categoryId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Filtered Product");
        _unitOfWorkMock.Verify(u => u.Products.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FiltersBy_SearchTerm()
    {
        var matchingProducts = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Laptop Pro", Price = 999m, Stock = 10 }
        };

        _unitOfWorkMock.Setup(u => u.Products.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(matchingProducts);

        var query = new GetProductsQuery(SearchTerm: "Laptop");
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Laptop Pro");
        _unitOfWorkMock.Verify(u => u.Products.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
    }
}
