using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, CancellationToken cancellationToken = default)
        => !await _context.Products.AnyAsync(p => p.Sku == sku, cancellationToken);

    public async Task<bool> IsBarcodeUniqueAsync(string barcode, CancellationToken cancellationToken = default)
        => !await _context.Products.AnyAsync(p => p.Barcode == barcode, cancellationToken);

    // Delegated methods to generic-like behavior
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products.FindAsync([id], cancellationToken);

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<Product>>(_context.Products.AsNoTracking());

    public void Update(Product entity) => _context.Products.Update(entity);
    public void Remove(Product entity) => _context.Products.Remove(entity);
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null) _context.Products.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);

    // Other interface methods throw NotImplementedException for brevity
    // Ideally implement all from IRepository, or create a BaseRepository
    {{ ... }}
}
