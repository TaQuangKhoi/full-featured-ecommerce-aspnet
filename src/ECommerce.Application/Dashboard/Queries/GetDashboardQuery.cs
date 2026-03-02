using ECommerce.Shared.DTOs;
using MediatR;

namespace ECommerce.Application.Dashboard.Queries;

public record GetDashboardQuery : IRequest<DashboardDto>;
