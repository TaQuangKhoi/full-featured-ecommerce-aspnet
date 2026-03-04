using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Categories.Commands;
using ECommerce.Application.Categories.Queries;
using ECommerce.Shared.DTOs;

namespace ECommerce.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Area("Admin")]
[Route("Admin/Categories/[action]")]
public class AdminCategoriesController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<AdminCategoriesController> _logger;

    public AdminCategoriesController(ISender sender, ILogger<AdminCategoriesController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("[AdminCategories] Category list requested.");
        var categories = await _sender.Send(new GetCategoriesQuery());
        _logger.LogInformation("[AdminCategories] Category list returned {Count} items.", categories.Count);
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AdminCategories] Create category validation failed for Name={Name}", dto.Name);
            var categories = await _sender.Send(new GetCategoriesQuery());
            return View(nameof(Index), categories);
        }

        _logger.LogInformation("[AdminCategories] Creating category Name={Name}, Slug={Slug}, ParentId={ParentId}",
            dto.Name, dto.Slug, dto.ParentId);

        var id = await _sender.Send(new CreateCategoryCommand(
            Name: dto.Name,
            Slug: dto.Slug,
            ParentId: dto.ParentId));

        _logger.LogInformation("[AdminCategories] Category created successfully. CategoryId={CategoryId}", id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("[AdminCategories] Deleting category CategoryId={CategoryId}", id);

        var success = await _sender.Send(new DeleteCategoryCommand(id));

        if (success)
            _logger.LogInformation("[AdminCategories] Category deleted successfully. CategoryId={CategoryId}", id);
        else
            _logger.LogWarning("[AdminCategories] Category delete returned false (not found?). CategoryId={CategoryId}", id);

        return RedirectToAction(nameof(Index));
    }
}
