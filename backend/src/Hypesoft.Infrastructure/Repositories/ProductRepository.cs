using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Infrastructure.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Métodos específicos de IProductRepository
    public async Task<bool> IsSkuUniqueAsync(string sku, CancellationToken cancellationToken = default)
        => !await _context.Products.AnyAsync(p => p.Sku == sku, cancellationToken);

    public async Task<bool> IsBarcodeUniqueAsync(string barcode, CancellationToken cancellationToken = default)
        => !await _context.Products.AnyAsync(p => p.Barcode == barcode, cancellationToken);

    // Sobrescreva métodos de RepositoryBase<Product>, se necessário
    public override async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(entity, cancellationToken);
        return entity;
    }

    public override async Task AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddRangeAsync(entities, cancellationToken);
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products.FindAsync([id], cancellationToken);

    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Products.AsNoTracking().ToListAsync(cancellationToken);

    public override void Update(Product entity) => _context.Products.Update(entity);

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public override void Remove(Product entity) => _context.Products.Remove(entity);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public override async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null) _context.Products.Remove(entity);
    }

    public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await _context.Products.CountAsync(cancellationToken);

    public override async Task<int> CountAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.Products.CountAsync(predicate, cancellationToken);

    public override async Task<bool> ExistsAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.Products.AnyAsync(predicate, cancellationToken);
}