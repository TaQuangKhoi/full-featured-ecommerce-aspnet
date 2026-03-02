using MediatR;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Categories.Queries;
using ECommerce.Application.Products.Queries;

namespace ECommerce.Web.Controllers;

public class ProductsController : Controller
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index(int page = 1, Guid? categoryId = null, string? search = null)
    {
        var products = await _sender.Send(new GetProductsQuery(
            Page: page,
            PageSize: 12,
            CategoryId: categoryId,
            SearchTerm: search));

        var categories = await _sender.Send(new GetCategoriesQuery());
        ViewBag.Categories = categories;
        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.CurrentSearch = search;

        return View(products);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id));
        if (product is null)
            return NotFound();

        return View(product);
    }
}
