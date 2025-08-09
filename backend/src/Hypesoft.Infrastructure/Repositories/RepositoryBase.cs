using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;

namespace Hypesoft.Infrastructure.Repositories;

public class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public RepositoryBase(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, string includeProperties, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;
        query = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Aggregate(query, (current, include) => current.Include(include.Trim()));
        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> query = _dbSet;
        query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(cancellationToken);

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(predicate, cancellationToken);

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, string? includeProperties = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;
        if (predicate != null)
            query = query.Where(predicate);
        if (!string.IsNullOrWhiteSpace(includeProperties))
            query = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Aggregate(query, (current, include) => current.Include(include.Trim()));
        if (orderBy != null)
            query = orderBy(query);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(cancellationToken);
        return (items, total);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    public virtual void Update(TEntity entity) => _dbSet.Update(entity);

    public virtual void UpdateRange(IEnumerable<TEntity> entities) => _dbSet.UpdateRange(entities);

    public virtual void Remove(TEntity entity) => _dbSet.Remove(entity);

    public virtual void RemoveRange(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);

    public virtual async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null) _dbSet.Remove(entity);
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);

    public virtual void Detach(TEntity entity) => _context.Entry(entity).State = EntityState.Detached;

    public virtual void DetachAll()
    {
        var entries = _context.ChangeTracker.Entries().ToList();
        foreach (var entry in entries) entry.State = EntityState.Detached;
    }

    public virtual async Task<IEnumerable<TEntity>> FromSqlAsync(string sql, params object[] parameters)
        => await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();

    public virtual Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        => _context.Database.ExecuteSqlRawAsync(sql, parameters);
}