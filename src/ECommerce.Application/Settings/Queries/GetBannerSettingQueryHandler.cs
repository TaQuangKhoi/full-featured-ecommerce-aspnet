using AutoMapper;
using ECommerce.Domain.Interfaces;
using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Settings.Queries;

public class GetBannerSettingQueryHandler : IRequestHandler<GetBannerSettingQuery, BannerSettingDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBannerSettingQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BannerSettingDto?> Handle(GetBannerSettingQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.BannerSettings.GetAllAsync();
        var banner = all.FirstOrDefault();
        return banner is null ? null : _mapper.Map<BannerSettingDto>(banner);
    }
}
