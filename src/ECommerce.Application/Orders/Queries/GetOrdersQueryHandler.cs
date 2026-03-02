using AutoMapper;
using ECommerce.Domain.Interfaces;
using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Orders.Queries;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = !string.IsNullOrWhiteSpace(request.CustomerId)
            ? await _unitOfWork.Orders.FindAsync(o => o.CustomerId == request.CustomerId)
            : await _unitOfWork.Orders.GetAllAsync();

        var orderList = orders.ToList();
        var totalCount = orderList.Count;

        var paged = orderList
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(paged),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
