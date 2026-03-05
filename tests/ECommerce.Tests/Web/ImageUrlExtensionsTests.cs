using ECommerce.Web.Extensions;
using FluentAssertions;

namespace ECommerce.Tests.Web;

public class ImageUrlExtensionsTests
{
    [Fact]
    public void ToHostedImageUrl_WithRootRelativePath_ReturnsHostedAbsoluteUrl()
    {
        var result = "/images/products/test.jpg".ToHostedImageUrl();

        result.Should().Be("http://full-featured-ecommerce-aspnet.runasp.net/images/products/test.jpg");
    }

    [Fact]
    public void ToHostedImageUrl_WithAbsoluteHttpsUrl_ReturnsOriginalUrl()
    {
        const string imageUrl = "https://cdn.example.com/products/test.jpg";

        var result = imageUrl.ToHostedImageUrl();

        result.Should().Be(imageUrl);
    }

    [Fact]
    public void ToHostedImageUrl_WithAbsoluteHttpUrl_ReturnsOriginalUrl()
    {
        const string imageUrl = "http://cdn.example.com/products/test.jpg";

        var result = imageUrl.ToHostedImageUrl();

        result.Should().Be(imageUrl);
    }

    [Fact]
    public void ToHostedImageUrl_WithProtocolRelativeUrl_ReturnsOriginalUrl()
    {
        const string imageUrl = "//cdn.example.com/products/test.jpg";

        var result = imageUrl.ToHostedImageUrl();

        result.Should().Be(imageUrl);
    }

    [Fact]
    public void ToHostedImageUrl_WithRelativePathWithoutLeadingSlash_ReturnsHostedAbsoluteUrl()
    {
        var result = "images/products/test.jpg".ToHostedImageUrl();

        result.Should().Be("http://full-featured-ecommerce-aspnet.runasp.net/images/products/test.jpg");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToHostedImageUrl_WithNullOrWhitespace_ReturnsEmptyString(string? imageUrl)
    {
        var result = imageUrl.ToHostedImageUrl();

        result.Should().BeEmpty();
    }
}
