namespace ECommerce.Shared.DTOs;

public class DashboardDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public List<OrderDto> RecentOrders { get; set; } = new();
}
