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

    public AdminCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        var categories = await _sender.Send(new GetCategoriesQuery());
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _sender.Send(new GetCategoriesQuery());
            return View(nameof(Index), categories);
        }

        await _sender.Send(new CreateCategoryCommand(
            Name: dto.Name,
            Slug: dto.Slug,
            ParentId: dto.ParentId));

        return RedirectToAction(nameof(Index));
    }
}
