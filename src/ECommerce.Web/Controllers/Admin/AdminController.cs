using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Dashboard.Queries;

namespace ECommerce.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Area("Admin")]
[Route("Admin/[action]")]
public class AdminController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ISender sender, ILogger<AdminController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("[Admin] Dashboard requested by UserId={UserId}",
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

        var dashboard = await _sender.Send(new GetDashboardQuery());

        _logger.LogInformation("[Admin] Dashboard loaded. TotalOrders={TotalOrders}, TotalProducts={TotalProducts}",
            dashboard.TotalOrders, dashboard.TotalProducts);

        return View(dashboard);
    }
}
