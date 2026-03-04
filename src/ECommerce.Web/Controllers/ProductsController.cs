using MediatR;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Categories.Queries;
using ECommerce.Application.Products.Queries;

namespace ECommerce.Web.Controllers;

public class ProductsController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ISender sender, ILogger<ProductsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, Guid? categoryId = null, string? search = null)
    {
        _logger.LogDebug("[Products] Index requested. Page={Page}, CategoryId={CategoryId}, Search={Search}",
            page, categoryId, search);

        var products = await _sender.Send(new GetProductsQuery(
            Page: page,
            PageSize: 12,
            CategoryId: categoryId,
            SearchTerm: search));

        _logger.LogInformation("[Products] Index returned {Count} products (page {Page}).",
            products.Items.Count, page);

        var categories = await _sender.Send(new GetCategoriesQuery());
        ViewBag.Categories = categories;
        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.CurrentSearch = search;

        return View(products);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        _logger.LogDebug("[Products] Details requested for ProductId={ProductId}", id);

        var product = await _sender.Send(new GetProductByIdQuery(id));
        if (product is null)
        {
            _logger.LogWarning("[Products] Product not found. ProductId={ProductId}", id);
            return NotFound();
        }

        _logger.LogInformation("[Products] Product found. ProductId={ProductId}, Name={Name}", product.Id, product.Name);
        return View(product);
    }
}
