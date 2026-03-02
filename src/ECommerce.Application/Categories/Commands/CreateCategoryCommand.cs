using MediatR;

namespace ECommerce.Application.Categories.Commands;

public record CreateCategoryCommand(
    string Name,
    string Slug,
    Guid? ParentId = null) : IRequest<Guid>;
