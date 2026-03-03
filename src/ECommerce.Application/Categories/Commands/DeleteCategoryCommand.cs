using MediatR;

namespace ECommerce.Application.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<bool>;
