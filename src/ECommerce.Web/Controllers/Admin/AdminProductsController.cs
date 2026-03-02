using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Categories.Queries;
using ECommerce.Application.Products.Commands;
using ECommerce.Application.Products.Queries;

namespace ECommerce.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Area("Admin")]
[Route("Admin/Products/[action]")]
public class AdminProductsController : Controller
{
    private readonly ISender _sender;

    public AdminProductsController(ISender sender)
    {
        _sender = sender;
    }

    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        var products = await _sender.Send(new GetProductsQuery(PageSize: 50));
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
            return View(command);
        }

        await _sender.Send(command);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id));
        if (product is null)
            return NotFound();

        ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            Stock: product.Stock,
            ImageUrl: product.ImageUrl,
            CategoryId: product.CategoryId,
            IsActive: product.IsActive);

        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProductCommand command)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
            return View(command);
        }

        await _sender.Send(command);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteProductCommand(id));
        return RedirectToAction(nameof(Index));
    }
}
