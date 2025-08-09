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

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products.FindAsync([id], cancellationToken);

    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<Product>>(_context.Products.AsNoTracking());

    public void Update(Product entity) => _context.Products.Update(entity);
    
    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public void Remove(Product entity) => _context.Products.Remove(entity);
    
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        
        _context.Products.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
    
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null) _context.Products.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
    
    // Outros métodos da interface IRepository que podem ser implementados posteriormente
    public Task<int> CountAsync(CancellationToken cancellationToken = default) => _context.Products.CountAsync(cancellationToken);
    
    public Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        => _context.Products.CountAsync(predicate, cancellationToken);
        
    public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        => _context.Products.AnyAsync(predicate, cancellationToken);
}
