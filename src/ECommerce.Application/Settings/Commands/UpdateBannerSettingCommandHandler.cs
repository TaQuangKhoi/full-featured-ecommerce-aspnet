using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using MediatR;

namespace ECommerce.Application.Settings.Commands;

public class UpdateBannerSettingCommandHandler : IRequestHandler<UpdateBannerSettingCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBannerSettingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateBannerSettingCommand request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.BannerSettings.GetAllAsync();
        var banner = all.FirstOrDefault();

        if (banner is null)
        {
            banner = new BannerSetting
            {
                ImageUrl = request.ImageUrl,
                Title = request.Title,
                Subtitle = request.Subtitle
            };
            await _unitOfWork.BannerSettings.AddAsync(banner);
        }
        else
        {
            banner.ImageUrl = request.ImageUrl;
            banner.Title = request.Title;
            banner.Subtitle = request.Subtitle;
            banner.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.BannerSettings.UpdateAsync(banner);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
