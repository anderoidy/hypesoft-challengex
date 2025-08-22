using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
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
        protected readonly ISpecificationEvaluator _specificationEvaluator;

        public Repository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbSet = DbContext.Set<TEntity>();
            _specificationEvaluator = SpecificationEvaluator.Default;
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

        // ✅ IMPLEMENTAÇÃO CORRETA - ISpecification<TEntity> usando Ardalis
        public async Task<IReadOnlyList<TEntity>> ListAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var queryResult = await ApplySpecification(specification)
                .ToListAsync(cancellationToken);

            return queryResult.AsReadOnly();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        // ✅ IMPLEMENTAÇÃO CORRETA - ISpecification<TEntity, TResult> usando Ardalis
        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            Ardalis.Specification.ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var queryResult = await ApplySpecification(specification)
                .ToListAsync(cancellationToken);

            return queryResult.AsReadOnly();
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(
            Ardalis.Specification.ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        // ✅ MÉTODOS AUXILIARES usando Ardalis.Specification.EntityFrameworkCore
        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            return _specificationEvaluator.GetQuery(DbSet.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(
            Ardalis.Specification.ISpecification<TEntity, TResult> specification
        )
        {
            return _specificationEvaluator.GetQuery(DbSet.AsQueryable(), specification);
        }

        // ✅ IMPLEMENTAÇÃO DOS MÉTODOS DA INTERFACE CUSTOMIZADA
        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            Hypesoft.Domain.Common.Interfaces.ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = DbSet.AsQueryable();

            // Aplica critérios
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Aplica ordenação
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Aplica paginação
            if (specification.IsPagingEnabled)
            {
                if (specification.Skip > 0)
                {
                    query = query.Skip(specification.Skip);
                }

                if (specification.Take > 0)
                {
                    query = query.Take(specification.Take);
                }
            }

            // Aplica projeção se existir
            if (specification.Selector != null)
            {
                var projectedQuery = query.Select(specification.Selector);
                var result = await projectedQuery.ToListAsync(cancellationToken);
                return result.AsReadOnly();
            }

            // Se não há projeção, retorna como TEntity
            var entities = await query.ToListAsync(cancellationToken);
            return entities.Cast<TResult>().ToList().AsReadOnly();
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(
            Hypesoft.Domain.Common.Interfaces.ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = DbSet.AsQueryable();

            // Aplica critérios
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Aplica ordenação
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Aplica projeção se existir
            if (specification.Selector != null)
            {
                var projectedQuery = query.Select(specification.Selector);
                return await projectedQuery.FirstOrDefaultAsync(cancellationToken);
            }

            // Se não há projeção, retorna como TEntity
            var entity = await query.FirstOrDefaultAsync(cancellationToken);
            return entity != null ? (TResult)(object)entity : default;
        }
    }
}
