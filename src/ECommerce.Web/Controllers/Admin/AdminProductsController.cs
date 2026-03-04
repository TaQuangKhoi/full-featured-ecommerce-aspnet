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
    private readonly ILogger<AdminProductsController> _logger;

    public AdminProductsController(ISender sender, ILogger<AdminProductsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("[AdminProducts] Product list requested.");
        var products = await _sender.Send(new GetProductsQuery(PageSize: 50));
        _logger.LogInformation("[AdminProducts] Product list returned {Count} items.", products.Items.Count);
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        _logger.LogDebug("[AdminProducts] Create product form requested.");
        ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AdminProducts] Create product validation failed for Name={Name}", command.Name);
            ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
            return View(command);
        }

        _logger.LogInformation("[AdminProducts] Creating product Name={Name}, Price={Price}, CategoryId={CategoryId}",
            command.Name, command.Price, command.CategoryId);

        var id = await _sender.Send(command);

        _logger.LogInformation("[AdminProducts] Product created successfully. ProductId={ProductId}", id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        _logger.LogDebug("[AdminProducts] Edit form requested for ProductId={ProductId}", id);

        var product = await _sender.Send(new GetProductByIdQuery(id));
        if (product is null)
        {
            _logger.LogWarning("[AdminProducts] Product not found for edit. ProductId={ProductId}", id);
            return NotFound();
        }

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
            _logger.LogWarning("[AdminProducts] Edit product validation failed for ProductId={ProductId}", command.Id);
            ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
            return View(command);
        }

        _logger.LogInformation("[AdminProducts] Updating product ProductId={ProductId}, Name={Name}",
            command.Id, command.Name);

        var success = await _sender.Send(command);

        if (success)
            _logger.LogInformation("[AdminProducts] Product updated successfully. ProductId={ProductId}", command.Id);
        else
            _logger.LogWarning("[AdminProducts] Product update returned false. ProductId={ProductId}", command.Id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("[AdminProducts] Deleting product ProductId={ProductId}", id);

        var success = await _sender.Send(new DeleteProductCommand(id));

        if (success)
            _logger.LogInformation("[AdminProducts] Product deleted successfully. ProductId={ProductId}", id);
        else
            _logger.LogWarning("[AdminProducts] Product delete returned false (not found?). ProductId={ProductId}", id);

        return RedirectToAction(nameof(Index));
    }
}
