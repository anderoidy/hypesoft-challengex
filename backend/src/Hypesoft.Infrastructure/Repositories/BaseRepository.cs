using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Domain.Interfaces;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Repositories
{
    public abstract class BaseRepository<T> : RepositoryBase<T>, IDisposable
        where T : class
    {
        protected readonly ILogger _logger; // uso generico faz DI funcionar para qualquer repo

        protected BaseRepository(ApplicationDbContext context, ILogger logger)
            : base(context)
        {
            _logger = logger;
        }

        // Não precisa de construtor com DbContext extra, pois sempre usamos ApplicationDbContext

        // Métodos auxiliares específicos do domínio
        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().ToListAsync(cancellationToken); // Use DbContext
        }

        public virtual async Task<T?> GetByFieldAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> ListByFieldAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().AnyAsync(predicate, cancellationToken);
        }

        public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return DbContext.Set<T>().Where(predicate); // Use DbContext
        }

        // Métodos de logging com ILogger genérica
        protected void LogInfo(string message, params object[] args)
        {
            _logger?.LogInformation(message, args);
        }

        protected void LogError(Exception ex, string message, params object[] args)
        {
            _logger?.LogError(ex, message, args);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
