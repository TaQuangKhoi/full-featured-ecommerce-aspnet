using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class CartController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
