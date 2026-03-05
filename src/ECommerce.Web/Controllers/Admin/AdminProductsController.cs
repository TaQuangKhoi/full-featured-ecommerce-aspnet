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
    private readonly IWebHostEnvironment _environment;

    public AdminProductsController(ISender sender, ILogger<AdminProductsController> logger, IWebHostEnvironment environment)
    {
        _sender = sender;
        _logger = logger;
        _environment = environment;
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
    public async Task<IActionResult> Create(CreateProductCommand command, IFormFile? productImage)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AdminProducts] Create product validation failed for Name={Name}", command.Name);
            ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
            return View(command);
        }

        if (productImage is { Length: > 0 })
        {
            var imageUrl = await SaveProductImageAsync(productImage);
            if (imageUrl is null)
            {
                ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
                return View(command);
            }

            command = command with { ImageUrl = imageUrl };
            _logger.LogInformation("[AdminProducts] Product image uploaded: {ImageUrl}", imageUrl);
        }

        _logger.LogInformation("[AdminProducts] Creating product Name={Name}, Price={Price}, CategoryId={CategoryId}",
            command.Name, command.Price, command.CategoryId);

        var id = await _sender.Send(command);

        _logger.LogInformation("[AdminProducts] Product created successfully. ProductId={ProductId}", id);
        TempData["Success"] = "Product created successfully.";
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
    [Route("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateProductCommand command, IFormFile? productImage)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("[AdminProducts] Edit product id mismatch: route={RouteId}, form={FormId}", id, command.Id);
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AdminProducts] Edit product validation failed for ProductId={ProductId}", id);
            ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
            return View(command);
        }

        if (productImage is { Length: > 0 })
        {
            var imageUrl = await SaveProductImageAsync(productImage);
            if (imageUrl is null)
            {
                ViewBag.Categories = await _sender.Send(new GetCategoriesQuery());
                return View(command);
            }

            // Delete the old locally-stored product image to avoid orphaned files
            if (command.ImageUrl is { } oldUrl && oldUrl.StartsWith("/images/products/"))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, oldUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                    _logger.LogInformation("[AdminProducts] Deleted old product image: {OldUrl}", oldUrl);
                }
            }

            command = command with { ImageUrl = imageUrl };
            _logger.LogInformation("[AdminProducts] Product image uploaded: {ImageUrl}", imageUrl);
        }

        _logger.LogInformation("[AdminProducts] Updating product ProductId={ProductId}, Name={Name}", id, command.Name);

        var success = await _sender.Send(command);

        if (!success)
        {
            _logger.LogWarning("[AdminProducts] Product not found for update. ProductId={ProductId}", id);
            return NotFound();
        }

        _logger.LogInformation("[AdminProducts] Product updated successfully. ProductId={ProductId}", id);
        TempData["Success"] = "Product updated successfully.";
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

    private async Task<string?> SaveProductImageAsync(IFormFile productImage)
    {
        const long maxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        if (productImage.Length > maxFileSizeBytes)
        {
            ModelState.AddModelError(string.Empty, "File size must not exceed 5 MB.");
            return null;
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(productImage.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(string.Empty, "Only image files (jpg, jpeg, png, webp, gif) are allowed.");
            return null;
        }

        var productsDir = Path.Combine(_environment.WebRootPath, "images", "products");
        Directory.CreateDirectory(productsDir);

        var fileName = $"product_image_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(productsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await productImage.CopyToAsync(stream);

        return $"/images/products/{fileName}";
    }
}
