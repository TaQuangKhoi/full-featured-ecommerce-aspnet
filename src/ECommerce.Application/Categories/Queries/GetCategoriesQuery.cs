using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Categories.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>;
