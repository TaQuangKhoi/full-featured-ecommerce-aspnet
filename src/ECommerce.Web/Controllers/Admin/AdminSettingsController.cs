using ECommerce.Application.Settings.Commands;
using ECommerce.Application.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Area("Admin")]
[Route("Admin/Settings/[action]")]
public class AdminSettingsController : Controller
{
    private readonly ISender _sender;
    private readonly ILogger<AdminSettingsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AdminSettingsController(ISender sender, ILogger<AdminSettingsController> logger, IWebHostEnvironment environment)
    {
        _sender = sender;
        _logger = logger;
        _environment = environment;
    }

    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        _logger.LogDebug("[AdminSettings] Settings page requested.");
        var banner = await _sender.Send(new GetBannerSettingQuery());
        return View(banner);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(IFormFile? bannerImage, string? bannerImageUrl, string? title, string? subtitle)
    {
        _logger.LogInformation("[AdminSettings] Updating banner settings.");

        string? imageUrl = bannerImageUrl;

        if (bannerImage is { Length: > 0 })
        {
            const long maxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
            if (bannerImage.Length > maxFileSizeBytes)
            {
                ModelState.AddModelError(string.Empty, "File size must not exceed 5 MB.");
                var banner = await _sender.Send(new GetBannerSettingQuery());
                return View(nameof(Index), banner);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(bannerImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Only image files (jpg, jpeg, png, webp, gif) are allowed.");
                var banner = await _sender.Send(new GetBannerSettingQuery());
                return View(nameof(Index), banner);
            }

            var bannerDir = Path.Combine(_environment.WebRootPath, "images", "banner");
            Directory.CreateDirectory(bannerDir);

            var fileName = $"banner_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(bannerDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await bannerImage.CopyToAsync(stream);

            imageUrl = $"/images/banner/{fileName}";
            _logger.LogInformation("[AdminSettings] Banner image uploaded: {ImageUrl}", imageUrl);

            // Delete previous locally-stored banner image to avoid orphaned files
            var existing = await _sender.Send(new GetBannerSettingQuery());
            if (existing?.ImageUrl is { } oldUrl && oldUrl.StartsWith("/images/banner/"))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, oldUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                    _logger.LogInformation("[AdminSettings] Deleted old banner image: {OldUrl}", oldUrl);
                }
            }
        }

        await _sender.Send(new UpdateBannerSettingCommand(
            ImageUrl: imageUrl,
            Title: string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            Subtitle: string.IsNullOrWhiteSpace(subtitle) ? null : subtitle.Trim()));

        _logger.LogInformation("[AdminSettings] Banner settings updated successfully.");
        TempData["Success"] = "Banner settings updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
