using AutoMapper;
using ECommerce.Domain.Entities;
using ECommerce.Shared.DTOs;

namespace ECommerce.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty));

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentName, opt => opt.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null));

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.OrderItems))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty));

        CreateMap<CreateProductDto, Product>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<BannerSetting, BannerSettingDto>();
    }
}
