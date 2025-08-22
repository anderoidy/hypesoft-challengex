using System;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Data;

public class ApplicationDbContext : DbContext 
{
    private readonly IMongoDatabase _database;
    private bool _disposed = false;

    public ApplicationDbContext(IOptions<MongoDbSettings> settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);

        // Initialize collections with proper names
        Products = _database.GetCollection<Product>("products");
        Categories = _database.GetCollection<Category>("categories");
        Users = _database.GetCollection<ApplicationUser>("users");
        Roles = _database.GetCollection<ApplicationRole>("roles");
        UserRoles = _database.GetCollection<ApplicationUserRole>("user_roles");

        // Create indexes on first use
        _ = InitializeDatabaseAsync();
    }

    /// <inheritdoc />
    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity
    {
        return new Repositories.Repository<TEntity>(this);
    }

    public async Task<IDisposable> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (_session != null)
            return _session;

        _session = await _database.Client.StartSessionAsync(cancellationToken: cancellationToken);
        _session.StartTransaction();
        return _session;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session == null)
            throw new InvalidOperationException("No active transaction to commit");

        try
        {
            await _session.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            _session.Dispose();
            _session = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session == null)
            throw new InvalidOperationException("No active transaction to rollback");

        try
        {
            await _session.AbortTransactionAsync(cancellationToken);
        }
        finally
        {
            _session.Dispose();
            _session = null;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In MongoDB, changes are saved automatically when operations are performed
        // This method is kept for interface compatibility
        await Task.CompletedTask;
    }

    // Database initialization
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            await MongoDbIndexes.CreateIndexes(_database);
            await SeedInitialDataAsync();
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the application
            Console.WriteLine($"Error initializing database: {ex.Message}");
        }
    }

    // Initial data seeding
    private async Task SeedInitialDataAsync()
    {
        // Check if we already have admin role
        var adminRole = await Roles.Find(r => r.Name == "Admin").FirstOrDefaultAsync();
        if (adminRole == null)
        {
            adminRole = new ApplicationRole("Admin")
            {
                Description = "Administrator with full access",
                IsSystemRole = true,
                CreatedBy = "system",
            };
            await Roles.InsertOneAsync(adminRole);
        }

        // Check if we already have a default admin user
        var adminUser = await Users
            .Find(u => u.Email == "admin@hypesoft.com")
            .FirstOrDefaultAsync();
        if (adminUser == null)
        {
            // In a real app, you'd hash the password properly
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@hypesoft.com",
                NormalizedEmail = "ADMIN@HYPESOFT.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedBy = "system",
            };

            await Users.InsertOneAsync(adminUser);

            // Assign admin role
            var userRole = new ApplicationUserRole(adminUser.Id, adminRole.Id, "system");
            await UserRoles.InsertOneAsync(userRole);
        }
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

    public void Dispose()
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
