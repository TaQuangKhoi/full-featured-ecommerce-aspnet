namespace ECommerce.Domain.Entities;

public class BannerSetting : BaseEntity
{
    public string? ImageUrl { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
}
