using System;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork, IDisposable
{
    private readonly IMongoDatabase _database;
    private IClientSessionHandle? _session;
    private bool _disposed = false;

    public ApplicationDbContext(IOptions<MongoDbSettings> settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    // MongoDB Collections
    public IMongoCollection<Product> Products => GetCollection<Product>("Products");
    public IMongoCollection<Category> Categories => GetCollection<Category>("Categories");
    public IMongoCollection<ApplicationUser> Users => GetCollection<ApplicationUser>("Users");
    public IMongoCollection<ApplicationRole> Roles => GetCollection<ApplicationRole>("Roles");
    public IMongoCollection<ApplicationUserRole> UserRoles =>
        GetCollection<ApplicationUserRole>("UserRoles");

    private IMongoCollection<T> GetCollection<T>(string name)
        where T : class
    {
        return _database.GetCollection<T>(name);
    }

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity
    {
        var collectionName = typeof(TEntity).Name + "s";
        return new RepositoryBase<TEntity>(this, collectionName);
    }

    public async Task<IDisposable> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    )
    {
        _session = await _database.Client.StartSessionAsync(cancellationToken: cancellationToken);
        _session.StartTransaction();
        return _session;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session == null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _session.CommitTransactionAsync(cancellationToken);
        _session.Dispose();
        _session = null;
    }

    public void RollbackTransaction()
    {
        if (_session == null)
            return;

        _session.AbortTransaction();
        _session.Dispose();
        _session = null;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In MongoDB, changes are typically saved immediately
        // This method is kept for interface compatibility
        return await Task.FromResult(0);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _session?.Dispose();
            }
            _disposed = true;
        }
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
