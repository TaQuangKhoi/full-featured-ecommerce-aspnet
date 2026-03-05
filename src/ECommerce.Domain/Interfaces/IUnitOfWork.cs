using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Product> Products { get; }
    IRepository<Category> Categories { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<BannerSetting> BannerSettings { get; }
    Task<int> SaveChangesAsync();
}
