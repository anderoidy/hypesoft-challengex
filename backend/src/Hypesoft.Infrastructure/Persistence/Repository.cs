using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Persistence
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbSet = DbContext.Set<TEntity>();
        }

        public IUnitOfWork UnitOfWork => DbContext;

        public IQueryable<TEntity> GetAll() => DbSet.AsQueryable();

        public IQueryable<TEntity> Find(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        ) => DbSet.Where(predicate);

        public async Task<TEntity?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        ) => await DbSet.FindAsync(new object[] { id }, cancellationToken);

        public async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        ) => await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            await DbSet.AddAsync(entity, cancellationToken);

        public async Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default
        ) => await DbSet.AddRangeAsync(entities, cancellationToken);

        public void Update(TEntity entity) => DbSet.Update(entity);

        public void UpdateRange(IEnumerable<TEntity> entities) => DbSet.UpdateRange(entities);

        public void Remove(TEntity entity) => DbSet.Remove(entity);

        public void RemoveRange(IEnumerable<TEntity> entities) => DbSet.RemoveRange(entities);

        public async Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        ) => await DbSet.AnyAsync(predicate, cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken = default) =>
            await DbSet.CountAsync(cancellationToken);

        public async Task<int> CountAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        ) => await DbSet.CountAsync(predicate, cancellationToken);

        // Implementação para ISpecification<TEntity> (sem TResult)
        public async Task<IReadOnlyList<TEntity>> ListAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = DbSet.AsQueryable();

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            var result = await query.ToListAsync(cancellationToken);
            return result.AsReadOnly();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = DbSet.AsQueryable();

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        // Implementação para ISpecification<TEntity, TResult> (com TResult)
        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = DbSet.AsQueryable();

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            // Assumindo que ISpecification<T, TResult> tem um Selector
            if (specification.Selector != null)
            {
                var result = await query
                    .Select(specification.Selector)
                    .ToListAsync(cancellationToken);
                return result.AsReadOnly();
            }

            return new List<TResult>().AsReadOnly();
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(
            ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = DbSet.AsQueryable();

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            // Assumindo que ISpecification<T, TResult> tem um Selector
            if (specification.Selector != null)
                return await query
                    .Select(specification.Selector)
                    .FirstOrDefaultAsync(cancellationToken);

            return default(TResult);
        }
    }
}
