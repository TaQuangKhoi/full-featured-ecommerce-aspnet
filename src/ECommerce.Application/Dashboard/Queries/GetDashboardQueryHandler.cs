using AutoMapper;
using ECommerce.Domain.Interfaces;
using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Dashboard.Queries;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDashboardQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();
        var products = await _unitOfWork.Products.GetAllAsync();

        var orderList = orders.ToList();

        var recentOrders = orderList
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToList();

        return new DashboardDto
        {
            TotalOrders = orderList.Count,
            TotalRevenue = orderList.Sum(o => o.Total),
            TotalProducts = products.Count(),
            TotalCustomers = orderList.Select(o => o.CustomerId).Distinct().Count(),
            RecentOrders = _mapper.Map<List<OrderDto>>(recentOrders)
        };
    }
}
