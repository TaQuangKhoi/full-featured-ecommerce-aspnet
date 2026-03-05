using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Area("Admin")]
[Route("Admin/Cart/[action]")]
public class AdminCartController : Controller
{
    private readonly ILogger<AdminCartController> _logger;

    public AdminCartController(ILogger<AdminCartController> logger)
    {
        _logger = logger;
    }

    [ActionName("Index")]
    public IActionResult Index()
    {
        _logger.LogDebug("[AdminCart] Cart management page requested.");
        return View();
    }
}
