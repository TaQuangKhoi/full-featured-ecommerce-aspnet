namespace ECommerce.Web.Extensions;

public static class ImageUrlExtensions
{
    public static string ToHostedImageUrl(this string? imageUrl)
    {
        return imageUrl ?? string.Empty;
    }
}
