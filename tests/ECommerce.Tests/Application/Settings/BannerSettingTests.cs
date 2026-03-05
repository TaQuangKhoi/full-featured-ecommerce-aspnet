using AutoMapper;
using ECommerce.Application.Mappings;
using ECommerce.Application.Settings.Commands;
using ECommerce.Application.Settings.Queries;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ECommerce.Tests.Application.Settings;

public class BannerSettingTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;

    public BannerSettingTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    // --- GetBannerSettingQueryHandler ---

    [Fact]
    public async Task GetBannerSetting_ReturnsNull_WhenNoBannerExists()
    {
        _unitOfWorkMock.Setup(u => u.BannerSettings.GetAllAsync())
            .ReturnsAsync(new List<BannerSetting>());

        var handler = new GetBannerSettingQueryHandler(_unitOfWorkMock.Object, _mapper);
        var result = await handler.Handle(new GetBannerSettingQuery(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBannerSetting_ReturnsBannerDto_WhenBannerExists()
    {
        var banner = new BannerSetting
        {
            Id = Guid.NewGuid(),
            ImageUrl = "/images/banner/test.jpg",
            Title = "Sale Now On",
            Subtitle = "Up to 50% off"
        };

        _unitOfWorkMock.Setup(u => u.BannerSettings.GetAllAsync())
            .ReturnsAsync(new List<BannerSetting> { banner });

        var handler = new GetBannerSettingQueryHandler(_unitOfWorkMock.Object, _mapper);
        var result = await handler.Handle(new GetBannerSettingQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(banner.Id);
        result.ImageUrl.Should().Be("/images/banner/test.jpg");
        result.Title.Should().Be("Sale Now On");
        result.Subtitle.Should().Be("Up to 50% off");
    }

    // --- UpdateBannerSettingCommandHandler ---

    [Fact]
    public async Task UpdateBannerSetting_CreatesBanner_WhenNoneExists()
    {
        BannerSetting? addedBanner = null;

        _unitOfWorkMock.Setup(u => u.BannerSettings.GetAllAsync())
            .ReturnsAsync(new List<BannerSetting>());
        _unitOfWorkMock.Setup(u => u.BannerSettings.AddAsync(It.IsAny<BannerSetting>()))
            .Callback<BannerSetting>(b => addedBanner = b)
            .ReturnsAsync((BannerSetting b) => b);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateBannerSettingCommandHandler(_unitOfWorkMock.Object);
        var result = await handler.Handle(
            new UpdateBannerSettingCommand("/images/banner/hero.jpg", "Welcome", "Shop today"),
            CancellationToken.None);

        result.Should().BeTrue();
        addedBanner.Should().NotBeNull();
        addedBanner!.ImageUrl.Should().Be("/images/banner/hero.jpg");
        addedBanner.Title.Should().Be("Welcome");
        addedBanner.Subtitle.Should().Be("Shop today");
        _unitOfWorkMock.Verify(u => u.BannerSettings.AddAsync(It.IsAny<BannerSetting>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateBannerSetting_UpdatesExistingBanner()
    {
        var existing = new BannerSetting
        {
            Id = Guid.NewGuid(),
            ImageUrl = "/images/banner/old.jpg",
            Title = "Old Title",
            Subtitle = "Old Subtitle"
        };

        _unitOfWorkMock.Setup(u => u.BannerSettings.GetAllAsync())
            .ReturnsAsync(new List<BannerSetting> { existing });
        _unitOfWorkMock.Setup(u => u.BannerSettings.UpdateAsync(It.IsAny<BannerSetting>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateBannerSettingCommandHandler(_unitOfWorkMock.Object);
        var result = await handler.Handle(
            new UpdateBannerSettingCommand("/images/banner/new.jpg", "New Title", "New Subtitle"),
            CancellationToken.None);

        result.Should().BeTrue();
        existing.ImageUrl.Should().Be("/images/banner/new.jpg");
        existing.Title.Should().Be("New Title");
        existing.Subtitle.Should().Be("New Subtitle");
        _unitOfWorkMock.Verify(u => u.BannerSettings.UpdateAsync(existing), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateBannerSetting_AllowsNullImageUrl()
    {
        _unitOfWorkMock.Setup(u => u.BannerSettings.GetAllAsync())
            .ReturnsAsync(new List<BannerSetting>());
        _unitOfWorkMock.Setup(u => u.BannerSettings.AddAsync(It.IsAny<BannerSetting>()))
            .ReturnsAsync((BannerSetting b) => b);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new UpdateBannerSettingCommandHandler(_unitOfWorkMock.Object);
        var result = await handler.Handle(
            new UpdateBannerSettingCommand(null, null, null),
            CancellationToken.None);

        result.Should().BeTrue();
    }
}
