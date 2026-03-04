using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Shared.DTOs;

namespace ECommerce.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        _logger.LogDebug("[Account] Login page requested. ReturnUrl={ReturnUrl}", returnUrl);
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[Account] Login form validation failed for Email={Email}", model.Email);
            return View(model);
        }

        _logger.LogInformation("[Account] Login attempt for Email={Email}", model.Email);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("[Account] Login succeeded for Email={Email}. Redirecting to ReturnUrl={ReturnUrl}", model.Email, returnUrl);
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        if (result.IsLockedOut)
            _logger.LogWarning("[Account] Account locked out for Email={Email}", model.Email);
        else
            _logger.LogWarning("[Account] Login failed for Email={Email}. IsNotAllowed={IsNotAllowed}", model.Email, result.IsNotAllowed);

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        _logger.LogDebug("[Account] Register page requested.");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[Account] Register form validation failed for Email={Email}", model.Email);
            return View(model);
        }

        _logger.LogInformation("[Account] Registering new user Email={Email}", model.Email);

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("[Account] User registered successfully. UserId={UserId}, Email={Email}", user.Id, user.Email);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            _logger.LogWarning("[Account] Registration error for Email={Email}: [{Code}] {Description}", model.Email, error.Code, error.Description);
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("[Account] User logging out. UserId={UserId}", userId);
        await _signInManager.SignOutAsync();
        _logger.LogInformation("[Account] Logout successful for UserId={UserId}", userId);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogWarning("[Account] Access denied for UserId={UserId}, Path={Path}", userId, Request.Path);
        return View();
    }
}
