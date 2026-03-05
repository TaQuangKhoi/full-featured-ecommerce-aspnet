using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Settings.Queries;

public record GetBannerSettingQuery : IRequest<BannerSettingDto?>;
