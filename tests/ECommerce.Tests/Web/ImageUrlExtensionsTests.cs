using ECommerce.Web.Extensions;
using FluentAssertions;

namespace ECommerce.Tests.Web;

public class ImageUrlExtensionsTests
{
    [Fact]
    public void ToHostedImageUrl_WithRootRelativePath_ReturnsOriginalUrl()
    {
        const string imageUrl = "/images/products/test.jpg";

        var result = imageUrl.ToHostedImageUrl();

        result.Should().Be(imageUrl);
    }

    [Fact]
    public void ToHostedImageUrl_WithAbsoluteUrl_ReturnsOriginalUrl()
    {
        const string imageUrl = "https://cdn.example.com/products/test.jpg";

        var result = imageUrl.ToHostedImageUrl();

        result.Should().Be(imageUrl);
    }

    [Fact]
    public void ToHostedImageUrl_WithRelativePathWithoutLeadingSlash_ReturnsOriginalUrl()
    {
        const string imageUrl = "images/products/test.jpg";

        var result = imageUrl.ToHostedImageUrl();

        result.Should().Be(imageUrl);
    }

    [Fact]
    public void ToHostedImageUrl_WithNull_ReturnsEmptyString()
    {
        string? imageUrl = null;

        var result = imageUrl.ToHostedImageUrl();

        result.Should().BeEmpty();
    }
}
