using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IRepository<Product>? _products;
    private IRepository<Category>? _categories;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;
    private IRepository<BannerSetting>? _bannerSettings;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Product> Products =>
        _products ??= new Repository<Product>(_context);

    public IRepository<Category> Categories =>
        _categories ??= new Repository<Category>(_context);

    public IRepository<Order> Orders =>
        _orders ??= new Repository<Order>(_context);

    public IRepository<OrderItem> OrderItems =>
        _orderItems ??= new Repository<OrderItem>(_context);

    public IRepository<BannerSetting> BannerSettings =>
        _bannerSettings ??= new Repository<BannerSetting>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
