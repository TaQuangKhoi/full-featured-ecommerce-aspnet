using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Products.Queries;
using ECommerce.Web.Models;

namespace ECommerce.Web.Controllers;

public class HomeController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ISender sender, ILogger<HomeController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _sender.Send(new GetProductsQuery(Page: 1, PageSize: 8));
        return View(products);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
