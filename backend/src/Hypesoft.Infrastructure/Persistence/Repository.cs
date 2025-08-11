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
    /// <summary>
    /// Represents a base class for all repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public Repository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbSet = DbContext.Set<TEntity>();
        }

        /// <inheritdoc />
        public IUnitOfWork UnitOfWork => DbContext;

        /// <inheritdoc />
        public IQueryable<TEntity> GetAll()
        {
            return DbSet.AsQueryable();
        }

        /// <inheritdoc />
        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return DbSet.Where(predicate);
        }

        /// <inheritdoc />
        public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DbSet.AddAsync(entity, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DbSet.AddRangeAsync(entities, cancellationToken);
        }

        /// <inheritdoc />
        public void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            DbSet.UpdateRange(entities);
        }

        /// <inheritdoc />
        public void Remove(TEntity entity)
        {
            // Soft delete if the entity implements ISoftDelete
            if (entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = DateTimeOffset.UtcNow;
                DbSet.Update(entity);
            }
            else
            {
                // Hard delete
                DbSet.Remove(entity);
            }
        }

        /// <inheritdoc />
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            var entityList = entities.ToList();
            
            // Handle soft delete for entities that implement ISoftDelete
            var softDeleteEntities = entityList.OfType<ISoftDelete>().ToList();
            if (softDeleteEntities.Any())
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var entity in softDeleteEntities.Cast<ISoftDelete>())
                {
                    entity.IsDeleted = true;
                    entity.DeletedAt = now;
                }
                
                DbSet.UpdateRange(softDeleteEntities.Cast<TEntity>());
                
                // Remove any non-soft-deletable entities
                var remainingEntities = entityList.Except(softDeleteEntities.Cast<TEntity>()).ToList();
                if (remainingEntities.Any())
                {
                    DbSet.RemoveRange(remainingEntities);
                }
            }
            else
            {
                // No soft-deletable entities, just remove all
                DbSet.RemoveRange(entityList);
            }
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DbSet.CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DbSet.CountAsync(predicate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await ApplySpecification(specification).ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await ApplySpecification(specification).ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Applies the given specification to the query.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>An <see cref="IQueryable{TEntity}"/> with the specification applied.</returns>
        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), specification);
        }

        /// <summary>
        /// Applies the given specification to the query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="specification">The specification.</param>
        /// <returns>An <see cref="IQueryable{TResult}"/> with the specification applied.</returns>
        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (specification.Selector == null) throw new ArgumentNullException(nameof(specification.Selector));

            return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), specification);
        }
    }
}
