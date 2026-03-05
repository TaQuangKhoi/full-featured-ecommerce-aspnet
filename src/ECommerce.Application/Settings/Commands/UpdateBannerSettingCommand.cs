using MediatR;

namespace ECommerce.Application.Settings.Commands;

public record UpdateBannerSettingCommand(
    string? ImageUrl,
    string? Title,
    string? Subtitle) : IRequest<bool>;
