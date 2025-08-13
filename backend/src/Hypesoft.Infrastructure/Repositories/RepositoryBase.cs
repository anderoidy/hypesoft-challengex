using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Hypesoft.Infrastructure.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T>
        where T : BaseEntity
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly ApplicationDbContext _context;

        public IUnitOfWork UnitOfWork => _context;

        protected RepositoryBase(ApplicationDbContext context, string collectionName)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _collection = GetCollection<T>(collectionName);
        }

        protected IMongoCollection<TEntity> GetCollection<TEntity>(string name)
            where TEntity : class
        {
            return _context
                    .GetType()
                    .GetProperties()
                    .FirstOrDefault(p =>
                        p.PropertyType == typeof(IMongoCollection<TEntity>) && p.Name == name
                    )
                    ?.GetValue(_context) as IMongoCollection<TEntity>
                ?? throw new InvalidOperationException($"Collection {name} not found in DbContext");
        }

        public virtual IQueryable<T> GetAll() => _collection.AsQueryable();

        public virtual IQueryable<T> Find(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default
        ) => _collection.AsQueryable().Where(predicate);

        public virtual async Task<T?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        ) => await _collection.Find(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);

        public virtual async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default
        ) => await _collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);

        public virtual async Task AddAsync(
            T entity,
            CancellationToken cancellationToken = default
        ) => await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

        public virtual async Task AddRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default
        ) => await _collection.InsertManyAsync(entities, cancellationToken: cancellationToken);

        public virtual void Update(T entity) =>
            _collection.ReplaceOne(e => e.Id == entity.Id, entity);

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
                Update(entity);
        }

        public virtual void Remove(T entity) => _collection.DeleteOne(e => e.Id == entity.Id);

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            var ids = entities.Select(e => e.Id).ToList();
            _collection.DeleteMany(e => ids.Contains(e.Id));
        }

        public virtual async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default
        ) => await _collection.Find(predicate).AnyAsync(cancellationToken);

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var count = await _collection.CountDocumentsAsync(
                FilterDefinition<T>.Empty,
                cancellationToken: cancellationToken
            );
            return (int)count;
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default
        )
        {
            var count = await _collection.CountDocumentsAsync(predicate, cancellationToken);
            return (int)count;
        }

        // Implementação para ISpecification<T, T>
        public virtual async Task<IReadOnlyList<T>> ListAsync(
            ISpecification<T, T> specification,
            CancellationToken cancellationToken = default
        )
        {
            var query = _collection.AsQueryable();

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);

            if (specification.IsPagingEnabled)
                query = query.Skip(specification.Skip).Take(specification.Take);

            var result = await query.ToListAsync(cancellationToken);
            return result.AsReadOnly();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(
            ISpecification<T, T> specification,
            CancellationToken cancellationToken = default
        )
        {
            var query = _collection.AsQueryable();

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        // Implementação para ISpecification<T, TResult> (com TResult)
        public virtual async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            ISpecification<T, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            var query = _collection.AsQueryable();

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

        public virtual async Task<TResult?> FirstOrDefaultAsync<TResult>(
            ISpecification<T, TResult> specification,
            CancellationToken cancellationToken = default
        )
        {
            var query = _collection.AsQueryable();

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            // Assumindo que ISpecification<T, TResult> tem um Selector
            if (specification.Selector != null)
                return await query
                    .Select(specification.Selector)
                    .FirstOrDefaultAsync(cancellationToken);

            return default(TResult);
        }

        // Implementação dos métodos da interface IRepository
        public virtual async Task<IReadOnlyList<T>> ListAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default
        )
        {
            return await ListAsync<T>(spec, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            ISpecification<T, TResult> spec,
            CancellationToken cancellationToken = default
        )
        {
            var query = _collection.AsQueryable();

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                query = spec.OrderBy(query);
            }

            if (spec.Skip.HasValue)
            {
                query = query.Skip(spec.Skip.Value);
            }

            if (spec.Take.HasValue)
            {
                query = query.Take(spec.Take.Value);
            }

            var result = await query.ToListAsync(cancellationToken);

            if (spec.Selector != null)
            {
                return result.Select(spec.Selector.Compile()).ToList();
            }

            return (IReadOnlyList<TResult>)result;
        }

        public virtual async Task<T?> FirstOrDefaultAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default
        )
        {
            var query = _collection.AsQueryable();

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                query = spec.OrderBy(query);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<TResult?> FirstOrDefaultAsync<TResult>(
            ISpecification<T, TResult> spec,
            CancellationToken cancellationToken = default
        )
        {
            var query = _collection.AsQueryable();

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                query = spec.OrderBy(query);
            }

            var result = await query.FirstOrDefaultAsync(cancellationToken);

            if (result == null || spec.Selector == null)
            {
                return default;
            }

            return spec.Selector.Compile()(result);
        }
    }
}
