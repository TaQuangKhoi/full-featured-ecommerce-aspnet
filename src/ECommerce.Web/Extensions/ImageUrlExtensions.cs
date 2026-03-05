namespace ECommerce.Web.Extensions;

public static class ImageUrlExtensions
{
    private const string HostedImageBaseUrl = "http://full-featured-ecommerce-aspnet.runasp.net";

    public static string ToHostedImageUrl(this string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return string.Empty;
        }

        if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            imageUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return imageUrl;
        }

        return imageUrl.StartsWith("/")
            ? $"{HostedImageBaseUrl}{imageUrl}"
            : $"{HostedImageBaseUrl}/{imageUrl}";
    }
}
