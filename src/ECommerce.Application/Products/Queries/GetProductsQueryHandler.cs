using AutoMapper;
using ECommerce.Domain.Interfaces;
using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Products.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = request.CategoryId.HasValue || !string.IsNullOrWhiteSpace(request.SearchTerm)
            ? await _unitOfWork.Products.FindAsync(p =>
                (!request.CategoryId.HasValue || p.CategoryId == request.CategoryId.Value) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) || p.Name.Contains(request.SearchTerm)))
            : await _unitOfWork.Products.GetAllAsync();

        var productList = products.ToList();
        var totalCount = productList.Count;

        var paged = productList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(paged),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
